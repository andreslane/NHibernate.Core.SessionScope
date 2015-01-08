namespace GNaP.Data.Scope.NHibernate.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.ExceptionServices;
    using global::NHibernate;

    public class NHibernateContextCollection
    {
        private readonly bool readOnly;
        private readonly IsolationLevel? isolationLevel;
        private readonly IDictionary<string, ISession> initializedSessions;
        private Dictionary<ISession, ITransaction> transactions;
        private bool disposed;
        private bool completed;

        internal IDictionary<string, ISession> InitializedDbContexts { get { return initializedSessions; } }

        public NHibernateContextCollection(bool readOnly = false, IsolationLevel? isolationLevel = null)
        {
            this.readOnly = readOnly;
            this.isolationLevel = isolationLevel;

            initializedSessions = new Dictionary<string, ISession>();
            transactions = new Dictionary<ISession, ITransaction>();
            disposed = false;
            completed = false;
        }

        public TDbContext Get<TDbContext, TFactory>() where TDbContext : ISession
        {
            if (disposed)
                throw new ObjectDisposedException("DbContextCollection");

            var sessionKey = typeof(TFactory).Name;

            if (!initializedSessions.ContainsKey(sessionKey))
            {
                // First time we've been asked for this particular DbContext type.
                // Create one, cache it and start its database transaction if needed.
                var contextFactory = (IContextFactory<ISessionFactory>)Activator.CreateInstance<TFactory>();
                ISessionFactory sessionFactory = contextFactory.Create();
                ISession session = sessionFactory.OpenSession();
                global::NHibernate.Context.CurrentSessionContext.Bind(session);

                initializedSessions.Add(sessionKey, session);

                if (readOnly)
                {
                    session.FlushMode = FlushMode.Never;
                }

                if (isolationLevel.HasValue)
                {
                    var transaction = session.BeginTransaction(isolationLevel.Value);
                    transactions.Add(session, transaction);
                }
            }

            return (TDbContext)initializedSessions[sessionKey];
        }

        public ISession GetFromFactory<TDbContextFactory>()
        {
            return Get<ISession, TDbContextFactory>();
        }

        public TDbContext Get<TDbContext>() where TDbContext : ISession
        {
            throw new NotSupportedException("Please use the Get<TDbContext, TDbContextFactory> method instead");
        }

        public void Dispose()
        {
            if (disposed)
                return;

            // Do our best here to dispose as much as we can even if we get errors along the way.
            // Now is not the time to throw. Correctly implemented applications will have called
            // either Commit() or Rollback() first and would have got the error there.

            if (!completed)
            {
                try
                {
                    if (readOnly) Commit();
                    else Rollback();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            foreach (var dbContext in initializedSessions.Values)
            {
                try
                {
                    dbContext.Dispose();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            initializedSessions.Clear();
            disposed = true;
        }

        public int Commit()
        {
            if (disposed)
                throw new ObjectDisposedException("DbContextCollection");
            if (completed)
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");

            // Best effort. You'll note that we're not actually implementing an atomic commit
            // here. It entirely possible that one DbContext instance will be committed successfully
            // and another will fail. Implementing an atomic commit would require us to wrap
            // all of this in a TransactionScope. The problem with TransactionScope is that
            // the database transaction it creates may be automatically promoted to a
            // distributed transaction if our DbContext instances happen to be using different
            // databases. And that would require the DTC service (Distributed Transaction Coordinator)
            // to be enabled on all of our live and dev servers as well as on all of our dev workstations.
            // Otherwise the whole thing would blow up at runtime.

            // In practice, if our services are implemented following a reasonably DDD approach,
            // a business transaction (i.e. a service method) should only modify entities in a single
            // DbContext. So we should never find ourselves in a situation where two DbContext instances
            // contain uncommitted changes here. We should therefore never be in a situation where the below
            // would result in a partial commit.

            ExceptionDispatchInfo lastError = null;

            var c = 0; // ??? is there a way to check the number of changes in NHibernate ???

            foreach (var session in initializedSessions.Values)
            {
                try
                {
                    if (!readOnly)
                    {
                        session.Flush();
                    }

                    // If we've started an explicit database transaction, time to commit it now.
                    var transaction = GetValueOrDefault(transactions, session);
                    if (transaction != null)
                    {
                        transaction.Commit();
                        transaction.Dispose();
                    }
                }
                catch (Exception e)
                {
                    lastError = ExceptionDispatchInfo.Capture(e);
                }
            }

            transactions.Clear();
            completed = true;

            if (lastError != null)
                lastError.Throw(); // Re-throw while maintaining the exception's original stack track

            return c;
        }

        public void Rollback()
        {
            if (disposed)
                throw new ObjectDisposedException("DbContextCollection");
            if (completed)
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");

            ExceptionDispatchInfo lastError = null;

            foreach (var dbContext in initializedSessions.Values)
            {
                // There's no need to explicitly rollback changes in a DbContext as
                // DbContext doesn't save any changes until its SaveChanges() method is called.
                // So "rolling back" for a DbContext simply means not calling its SaveChanges()
                // method.

                // But if we've started an explicit database transaction, then we must roll it back.
                var tran = GetValueOrDefault(transactions, dbContext);
                if (tran != null)
                {
                    try
                    {
                        tran.Rollback();
                        tran.Dispose();
                    }
                    catch (Exception e)
                    {
                        lastError = ExceptionDispatchInfo.Capture(e);
                    }
                }
            }

            transactions.Clear();
            completed = true;

            if (lastError != null)
                lastError.Throw(); // Re-throw while maintaining the exception's original stack track
        }


        private static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }
    }
}

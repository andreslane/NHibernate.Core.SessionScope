namespace GNaP.Data.Scope.NHibernate.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.ExceptionServices;
    using global::NHibernate;
    using Interfaces;

    internal class NHibernateSessionCollection
    {
        private readonly IDictionary<string, ISession> _initializedSessions;
        private readonly Dictionary<ISession, ITransaction> _transactions;
        private readonly IsolationLevel? _isolationLevel;
        private bool _disposed;
        private bool _completed;
        private readonly bool _readOnly;

        internal IDictionary<string, ISession> InitializedDbContexts { get { return _initializedSessions; } }

        public NHibernateSessionCollection(bool readOnly = false, IsolationLevel? isolationLevel = null)
        {
            _disposed = false;
            _completed = false;

            _initializedSessions = new Dictionary<string, ISession>();
            _transactions = new Dictionary<ISession, ITransaction>();

            _readOnly = readOnly;
            _isolationLevel = isolationLevel;
        }

        public TDbContext Get<TDbContext, TFactory>()
            where TDbContext : ISession
            where TFactory : IDbFactory<ISessionFactory>
        {
            if (_disposed)
                throw new ObjectDisposedException("NHibernateSessionCollection");

            var sessionKey = typeof(TFactory).Name;

            if (_initializedSessions.ContainsKey(sessionKey))
                return (TDbContext) _initializedSessions[sessionKey];

            // First time we've been asked for this particular DbContext type.
            // Create one, cache it and start its database transaction if needed.
            var contextFactory = Activator.CreateInstance<TFactory>();
            var sessionFactory = contextFactory.Create();
            var session = sessionFactory.OpenSession();
            global::NHibernate.Context.CurrentSessionContext.Bind(session);

            _initializedSessions.Add(sessionKey, session);

            if (_readOnly)
            {
                session.FlushMode = FlushMode.Never;
            }

            if (_isolationLevel.HasValue)
            {
                var transaction = session.BeginTransaction(_isolationLevel.Value);
                _transactions.Add(session, transaction);
            }

            return (TDbContext)_initializedSessions[sessionKey];
        }

        public ISession GetFromFactory<TDbContextFactory>()
            where TDbContextFactory : IDbFactory<ISessionFactory>
        {
            return Get<ISession, TDbContextFactory>();
        }

        public int Commit()
        {
            if (_disposed)
                throw new ObjectDisposedException("DbContextCollection");

            if (_completed)
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

            foreach (var session in _initializedSessions.Values)
            {
                try
                {
                    if (!_readOnly)
                    {
                        session.Flush();
                    }

                    // If we've started an explicit database transaction, time to commit it now.
                    var transaction = GetValueOrDefault(_transactions, session);
                    if (transaction == null)
                        continue;

                    transaction.Commit();
                    transaction.Dispose();
                }
                catch (Exception e)
                {
                    lastError = ExceptionDispatchInfo.Capture(e);
                }
            }

            _transactions.Clear();
            _completed = true;

            if (lastError != null)
                lastError.Throw(); // Re-throw while maintaining the exception's original stack track

            return c;
        }

        public void Rollback()
        {
            if (_disposed)
                throw new ObjectDisposedException("DbContextCollection");

            if (_completed)
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");

            ExceptionDispatchInfo lastError = null;

            foreach (var dbContext in _initializedSessions.Values)
            {
                // There's no need to explicitly rollback changes in a DbContext as
                // DbContext doesn't save any changes until its SaveChanges() method is called.
                // So "rolling back" for a DbContext simply means not calling its SaveChanges()
                // method.

                // But if we've started an explicit database transaction, then we must roll it back.
                var tran = GetValueOrDefault(_transactions, dbContext);
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

            _transactions.Clear();
            _completed = true;

            if (lastError != null)
                lastError.Throw(); // Re-throw while maintaining the exception's original stack track
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            // Do our best here to dispose as much as we can even if we get errors along the way.
            // Now is not the time to throw. Correctly implemented applications will have called
            // either Commit() or Rollback() first and would have got the error there.

            if (!_completed)
            {
                try
                {
                    if (_readOnly) Commit();
                    else Rollback();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            foreach (var dbContext in _initializedSessions.Values)
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

            _initializedSessions.Clear();
            _disposed = true;
        }

        private static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }
    }
}

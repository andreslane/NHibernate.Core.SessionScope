using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using NHibernate.SessionScope.Interfaces;

namespace NHibernate.SessionScope
{
    public class SessionScope : ISessionScope
    {
        private bool _disposed;
        private readonly bool _readOnly;
        private bool _completed;
        private readonly bool _nested;
        private readonly SessionScope _parentScope;
        private readonly Lazy<ISession> _session;

        public ISession Session => _session.Value;

        public SessionScope(ISessionFactory sessionFactory) : this(SessionScopeOption.JoinExisting, false, null, sessionFactory)
        {
        }

        public SessionScope(bool readOnly, ISessionFactory sessionFactory) : this(SessionScopeOption.JoinExisting, readOnly, null, sessionFactory)
        {
        }

        public SessionScope(SessionScopeOption joiningOption, bool readOnly, IsolationLevel? isolationLevel, ISessionFactory sessionFactory)
            : this(joiningOption, readOnly, isolationLevel, sessionFactory, null)
        {

        }

        public SessionScope(SessionScopeOption joiningOption, bool readOnly, IsolationLevel? isolationLevel, ISessionFactory sessionFactory, IInterceptor sessionLocalInterceptor)
        {
            if (isolationLevel.HasValue && joiningOption == SessionScopeOption.JoinExisting)
                throw new ArgumentException("Cannot join an ambient SessionScope when an explicit database transaction is required. When requiring explicit database transactions to be used (i.e. when the 'isolationLevel' parameter is set), you must not also ask to join the ambient context (i.e. the 'joinAmbient' parameter must be set to false).");

            if (sessionFactory == null)
                throw new ArgumentNullException(nameof(sessionFactory));

            _disposed = false;
            _completed = false;
            _readOnly = readOnly;

            _parentScope = GetAmbientScope();
            if (_parentScope != null && joiningOption == SessionScopeOption.JoinExisting)
            {
                if (_parentScope._readOnly && !_readOnly)
                {
                    throw new InvalidOperationException("Cannot nest a read/write SessionScope within a read-only SessionScope.");
                }

                _nested = true;
                _session = _parentScope._session;
            }
            else
            {
                _nested = false;
                _session = new Lazy<ISession>(() => {
                    ISession session;

                    if (sessionLocalInterceptor == null)
                    {
                        session = sessionFactory.OpenSession();
                    }
                    else
                    {
                        session = sessionFactory.WithOptions()
                            .Interceptor(sessionLocalInterceptor)
                            .OpenSession();
                    }                    

                    if (readOnly)
                    {
                        // REVIEW WC
                        session.DefaultReadOnly = true;
                    }

                    if (isolationLevel.HasValue)
                    {
                        session.BeginTransaction(isolationLevel.Value);
                    }
                    else
                    {
                        session.BeginTransaction();
                    }

                    return session;
                });
            }

            SetAmbientScope(this);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            // Commit / Rollback and dispose our ISession instance
            if (!_nested)
            {
                if (!_completed)
                {
                    // Do our best to clean up as much as we can but don't throw here as it's too late anyway.
                    try
                    {
                        if (_readOnly)
                        {
                            // Disposing a read-only scope before having called its SaveChanges() method
                            // is the normal and expected behavior. Read-only scopes get committed automatically.
                            CommitInternal();
                        }
                        else
                        {
                            // Disposing a read/write scope before having called its SaveChanges() method
                            // indicates that something went wrong and that all changes should be rolled-back.
                            RollbackInternal();
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }

                    _completed = true;
                }

                if (_session.IsValueCreated)
                {
                    if (_session.Value.IsOpen)
                        _session.Value.Close();

                    _session.Value.Dispose();
                }
            }

            // Pop ourself from the ambient scope stack
            SessionScope currentAmbientScope = GetAmbientScope();
            if (currentAmbientScope != this) // This is a serious programming error. Worth throwing here.
                throw new InvalidOperationException("SessionScope instances must be disposed of in the order in which they were created!");

            RemoveAmbientScope();

            if (_parentScope != null)
            {
                if (_parentScope._disposed)
                {
                    /*
                     * If our parent scope has been disposed before us, it can only mean one thing:
                     * someone started a parallel flow of execution and forgot to suppress the
                     * ambient context before doing so. And we've been created in that parallel flow.
                     * 
                     * Since the CallContext flows through all async points, the ambient scope in the 
                     * main flow of execution ended up becoming the ambient scope in this parallel flow
                     * of execution as well. So when we were created, we captured it as our "parent scope". 
                     * 
                     * The main flow of execution then completed while our flow was still ongoing. When 
                     * the main flow of execution completed, the ambient scope there (which we think is our 
                     * parent scope) got disposed of as it should.
                     * 
                     * So here we are: our parent scope isn't actually our parent scope. It was the ambient
                     * scope in the main flow of execution from which we branched off. We should never have seen 
                     * it. Whoever wrote the code that created this parallel task should have suppressed
                     * the ambient context before creating the task - that way we wouldn't have captured
                     * this bogus parent scope.
                     * 
                     * While this is definitely a programming error, it's not worth throwing here. We can only 
                     * be in one of two scenario:
                     * 
                     * - If the developer who created the parallel task was mindful to force the creation of 
                     * a new scope in the parallel task (with ISessionScopeFactory.CreateNew() instead of 
                     * JoinOrCreate()) then no harm has been done. We haven't tried to access the same ISession
                     * instance from multiple threads.
                     * 
                     * - If this was not the case, they probably already got an exception complaining about the same
                     * ISession being accessed from multiple threads simultaneously (or a related error like multiple
                     * active result sets on a DataReader, which is caused by attempting to execute several queries
                     * in parallel on the same ISession instance). So the code has already blow up.
                     * 
                     * So just record a warning here. Hopefully someone will see it and will fix the code.
                     */

                    string message = @"PROGRAMMING ERROR - When attempting to dispose a SessionScope, we found that our parent SessionScope has already been disposed! This means that someone started a parallel flow of execution (e.g. created a TPL task, created a thread or enqueued a work item on the ThreadPool) within the context of a SessionScope without suppressing the ambient context first.

In order to fix this:
1) Look at the stack trace below - this is the stack trace of the parallel task in question.
2) Find out where this parallel task was created.
3) Change the code so that the ambient context is suppressed before the parallel task is created. You can do this with ISessionScopeFactory.SuppressAmbientContext() (wrap the parallel task creation code block in this). 

Stack Trace:
" + Environment.StackTrace;

                    System.Diagnostics.Debug.WriteLine(message);
                }
                else
                {
                    SetAmbientScope(_parentScope);
                }
            }

            _disposed = true;
        }

        public void SaveChanges()
        {
            if (_disposed)
                throw new ObjectDisposedException("SessionScope");
            if (_completed)
                throw new InvalidOperationException("You cannot call SaveChanges() more than once on a SessionScope. A SessionScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own SessionScope and each implement a single business transaction.");

            // Only save changes if we're not a nested scope. Otherwise, let the top-level scope 
            // decide when the changes should be saved.
            if (!_nested)
            {
                CommitInternal();
            }

            _completed = true;
        }

        private void CommitInternal()
        {
            // In practice, if our services are implemented following a reasonably DDD approach,
            // a business transaction (i.e. a service method) should only modify entities in a single
            // ISession. So we should never find ourselves in a situation where two ISession instances
            // contain uncommitted changes here. We should therefore never be in a situation where the below
            // would result in a partial commit. 

            if (_session.IsValueCreated && _session.Value.Transaction != null && _session.Value.Transaction.IsActive)
            {
                try
                {
                    _session.Value.Transaction.Commit();
                }
                catch
                {
                    _session.Value.Transaction.Rollback();
                    throw;
                }
                finally
                {
                    _session.Value.Transaction.Dispose();
                }
            }
        }

        private void RollbackInternal()
        {
            if (_session.IsValueCreated && _session.Value.Transaction != null && _session.Value.Transaction.IsActive)
            {
                try
                {
                    _session.Value.Transaction.Rollback();
                }
                finally
                {
                    _session.Value.Transaction.Dispose();
                }
            }
        }

        #region Ambient Context Logic

        /*
         * This is where all the magic happens. And there is not much of it.
         * 
         * This implementation is inspired by the source code of the
         * TransactionScope class in .NET 4.5.1 (the TransactionScope class
         * in prior versions of the .NET Fx didn't have support for async
         * operations).
         * 
         * In order to understand this, you'll need to be familiar with the
         * concept of async points. You'll also need to be familiar with the
         * ExecutionContext and CallContext and understand how and why they 
         * flow through async points. Stephen Toub has written an
         * excellent blog post about this - it's a highly recommended read:
         * http://blogs.msdn.com/b/pfxteam/archive/2012/06/15/executioncontext-vs-synchronizationcontext.aspx
         * 
         * Overview: 
         * 
         * We want our SessionScope instances to be ambient within 
         * the context of a logical flow of execution. This flow may be 
         * synchronous or it may be asynchronous.
         * 
         * If we only wanted to support the synchronous flow scenario, 
         * we could just store our SessionScope instances in a ThreadStatic 
         * variable. That's the "traditional" (i.e. pre-async) way of implementing
         * an ambient context in .NET. You can see an example implementation of 
         * a TheadStatic-based ambient DbContext here: http://coding.abel.nu/2012/10/make-the-dbcontext-ambient-with-unitofworkscope/ 
         * 
         * But that would be hugely limiting as it would prevent us from being
         * able to use the new async features added to NHibernate en version 5
         * and .NET 4.5.
         * 
         * So we need a storage place for our SessionScope instances 
         * that can flow through async points so that the ambient context is still 
         * available after an await (or any other async point). And this is exactly 
         * what CallContext is for.
         * 
         * There are however two issues with storing our SessionScope instances 
         * in the CallContext:
         * 
         * 1) Items stored in the CallContext should be serializable. That's because
         * the CallContext flows not just through async points but also through app domain 
         * boundaries. I.e. if you make a remoting call into another app domain, the
         * CallContext will flow through this call (which will require all the values it
         * stores to get serialized) and get restored in the other app domain.
         * 
         * In our case, our SessionScope instances aren't serializable. And in any case,
         * we most definitely don't want them to be flown accross app domains. So we'll
         * use the trick used by the TransactionScope class to work around this issue.
         * Instead of storing our SessionScope instances themselves in the CallContext,
         * we'll just generate a unique key for each instance and only store that key in 
         * the CallContext. We'll then store the actual SessionScope instances in a static
         * Dictionary against their key. 
         * 
         * That way, if an app domain boundary is crossed, the keys will be flown accross
         * but not the SessionScope instances since a static variable is stored at the 
         * app domain level. The code executing in the other app domain won't see the ambient
         * SessionScope created in the first app domain and will therefore be able to create
         * their own ambient SessionScope if necessary.
         * 
         * 2) The CallContext is flow through *all* async points. This means that if someone
         * decides to create multiple threads within the scope of a SessionScope, our ambient scope
         * will flow through all the threads. Which means that all the threads will see that single 
         * SessionScope instance as being their ambient ISession. So clients need to be 
         * careful to always suppress the ambient context before kicking off a parallel operation
         * to avoid our ISession instances from being accessed from multiple threads.
         * 
         */



        // Use a ConditionalWeakTable instead of a simple ConcurrentDictionary to store our SessionScope instances 
        // in order to prevent leaking SessionScope instances if someone doesn't dispose them properly.
        //
        // For example, if we used a ConcurrentDictionary and someone let go of a SessionScope instance without 
        // disposing it, our ConcurrentDictionary would still have a reference to it, preventing
        // the GC from being able to collect it => leak. With a ConditionalWeakTable, we don't hold a reference
        // to the SessionScope instances we store in there, allowing them to get GCed.
        // The doc for ConditionalWeakTable isn't the best. This SO answer does a good job at explaining what 
        // it does: http://stackoverflow.com/a/18613811
        private static readonly ConditionalWeakTable<InstanceIdentifier, SessionScope> SessionScopeInstances = new ConditionalWeakTable<InstanceIdentifier, SessionScope>();

        private static readonly string AmbientDbContextScopeKey = "AmbientSession_" + Guid.NewGuid();


        private readonly InstanceIdentifier _instanceIdentifier = new InstanceIdentifier();

        /// <summary>
        /// Makes the provided 'SessionScope' available as the the ambient scope via the CallContext.
        /// </summary>
        internal static void SetAmbientScope(SessionScope newAmbientScope)
        {
            if (newAmbientScope == null)
                throw new ArgumentNullException(nameof(newAmbientScope));

            var current = CallContext.LogicalGetData(AmbientDbContextScopeKey) as InstanceIdentifier;

            if (current == newAmbientScope._instanceIdentifier)
                return;

            // Store the new scope's instance identifier in the CallContext, making it the ambient scope
            CallContext.LogicalSetData(AmbientDbContextScopeKey, newAmbientScope._instanceIdentifier);

            // Keep track of this instance (or do nothing if we're already tracking it)
            SessionScopeInstances.GetValue(newAmbientScope._instanceIdentifier, key => newAmbientScope);
        }

        /// <summary>
        /// Clears the ambient scope from the CallContext and stops tracking its instance. 
        /// Call this when a SessionScope is being disposed.
        /// </summary>
        internal static void RemoveAmbientScope()
        {
            // If there was an ambient scope, we can stop tracking it now
            if (CallContext.LogicalGetData(AmbientDbContextScopeKey) is InstanceIdentifier current)
            {
                SessionScopeInstances.Remove(current);
            }

            CallContext.LogicalSetData(AmbientDbContextScopeKey, null);
        }

        /// <summary>
        /// Clears the ambient scope from the CallContext but keeps tracking its instance. Call this to temporarily 
        /// hide the ambient context (e.g. to prevent it from being captured by parallel task).
        /// </summary>
        internal static void HideAmbientScope()
        {
            CallContext.LogicalSetData(AmbientDbContextScopeKey, null);
        }

        /// <summary>
        /// Get the current ambient scope or null if no ambient scope has been setup.
        /// </summary>
        internal static SessionScope GetAmbientScope()
        {
            // Retrieve the identifier of the ambient scope (if any)
            var instanceIdentifier = CallContext.LogicalGetData(AmbientDbContextScopeKey) as InstanceIdentifier;

            if (instanceIdentifier == null)
                return null; // Either no ambient context has been set or we've crossed an app domain boundary and have (intentionally) lost the ambient context

            // Retrieve the SessionScope instance corresponding to this identifier
            if (SessionScopeInstances.TryGetValue(instanceIdentifier, out SessionScope ambientScope))
                return ambientScope;

            // We have an instance identifier in the CallContext but no corresponding instance
            // in our SessionScopeInstances table. This should never happen! The only place where
            // we remove the instance from the SessionScopeInstances table is in RemoveAmbientScope(),
            // which also removes the instance identifier from the CallContext. 
            //
            // There's only one scenario where this could happen: someone let go of a SessionScope 
            // instance without disposing it. In that case, the CallContext
            // would still contain a reference to the scope and we'd still have that scope's instance
            // in our SessionScopeInstances table. But since we use a ConditionalWeakTable to store 
            // our SessionScope instances and are therefore only holding a weak reference to these instances, 
            // the GC would be able to collect it. Once collected by the GC, our ConditionalWeakTable will return
            // null when queried for that instance. In that case, we're OK. This is a programming error 
            // but our use of a ConditionalWeakTable prevented a leak.
            System.Diagnostics.Debug.WriteLine("Programming error detected. Found a reference to an ambient SessionScope in the CallContext but didn't have an instance for it in our SessionScopeInstances table. This most likely means that this SessionScope instance wasn't disposed of properly. SessionScope instance must always be disposed. Review the code for any SessionScope instance used outside of a 'using' block and fix it so that all SessionScope instances are disposed of.");
            return null;
        }

        #endregion
    }

    /*
     * The idea of using an object reference as our instance identifier 
     * instead of simply using a unique string (which we could have generated
     * with Guid.NewGuid() for example) comes from the TransactionScope
     * class. As far as I can make out, a string would have worked just fine.
     * I'm guessing that this is done for optimization purposes. Creating
     * an empty class is cheaper and uses up less memory than generating
     * a unique string.
    */
    internal class InstanceIdentifier : MarshalByRefObject
    {
    }
}

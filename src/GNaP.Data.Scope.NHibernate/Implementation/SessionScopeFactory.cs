using System;
using System.Data;
using NHibernate.SessionScope.Interfaces;

namespace NHibernate.SessionScope
{
    public class SessionScopeFactory : ISessionScopeFactory
    {
        private readonly ISessionFactory _sessionFactory;

        public SessionScopeFactory(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

        public ISessionScope Create(SessionScopeOption joiningOption = SessionScopeOption.JoinExisting, IInterceptor sessionLocalInterceptor = null)
        {
            return new SessionScope(joiningOption, false, null, _sessionFactory, sessionLocalInterceptor);
        }

        public ISessionReadOnlyScope CreateReadOnly(SessionScopeOption joiningOption = SessionScopeOption.JoinExisting, IInterceptor sessionLocalInterceptor = null)
        {
            return new SessionReadOnlyScope(joiningOption, null, _sessionFactory, sessionLocalInterceptor);
        }

        public ISessionReadOnlyScope CreateReadOnlyWithIsolationLevel(IsolationLevel isolationLevel, IInterceptor sessionLocalInterceptor = null)
        {
            return new SessionReadOnlyScope(SessionScopeOption.ForceCreateNew, isolationLevel, _sessionFactory, sessionLocalInterceptor);
        }

        public ISessionScope CreateWithIsolationLevel(IsolationLevel isolationLevel, IInterceptor sessionLocalInterceptor = null)
        {
            return new SessionScope(SessionScopeOption.ForceCreateNew, false, isolationLevel, _sessionFactory, sessionLocalInterceptor);
        }

        public IDisposable SuppressAmbientScope()
        {
            return new AmbientContextSuppressor();
        }
    }
}

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

        public ISessionScope Create(SessionScopeOption joiningOption = SessionScopeOption.JoinExisting)
        {
            return new SessionScope(joiningOption, false, null, _sessionFactory);
        }

        public ISessionReadOnlyScope CreateReadOnly(SessionScopeOption joiningOption = SessionScopeOption.JoinExisting)
        {
            return new SessionReadOnlyScope(joiningOption, null, _sessionFactory);
        }

        public ISessionReadOnlyScope CreateReadOnlyWithIsolationLevel(IsolationLevel isolationLevel)
        {
            return new SessionReadOnlyScope(SessionScopeOption.ForceCreateNew, isolationLevel, _sessionFactory);
        }

        public ISessionScope CreateWithIsolationLevel(IsolationLevel isolationLevel)
        {
            return new SessionScope(SessionScopeOption.ForceCreateNew, false, isolationLevel, _sessionFactory);
        }

        public IDisposable SuppressAmbientScope()
        {
            return new AmbientContextSuppressor();
        }
    }
}

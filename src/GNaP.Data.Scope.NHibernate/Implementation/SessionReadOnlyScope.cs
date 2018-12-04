/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

using System.Data;
using NHibernate.SessionScope.Interfaces;

namespace NHibernate.SessionScope
{
    public class SessionReadOnlyScope : ISessionReadOnlyScope
    {
        private readonly SessionScope _internalScope;

        public ISession Session => _internalScope.Session;

        public SessionReadOnlyScope(ISessionFactory sessionFactory)
            : this(SessionScopeOption.JoinExisting, null, sessionFactory)
        {
        }

        public SessionReadOnlyScope(IsolationLevel isolationLevel, ISessionFactory sessionFactory)
            : this(SessionScopeOption.ForceCreateNew, isolationLevel, sessionFactory)
        {
        }

        public SessionReadOnlyScope(SessionScopeOption joiningOption, IsolationLevel? isolationLevel, ISessionFactory sessionFactory)
            : this(joiningOption, isolationLevel, sessionFactory, null)
        {
        }

        public SessionReadOnlyScope(SessionScopeOption joiningOption, IsolationLevel? isolationLevel, ISessionFactory sessionFactory, IInterceptor sessionLocalInterceptor)
        {
            _internalScope = new SessionScope(joiningOption, true, isolationLevel, sessionFactory, sessionLocalInterceptor);
        }

        public void Dispose()
        {
            _internalScope.Dispose();
        }
    }
}

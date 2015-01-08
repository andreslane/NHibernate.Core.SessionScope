/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

namespace GNaP.Data.Scope.NHibernate.Implementation
{
    using System.Data;
    using global::NHibernate;
    using Interfaces;

    public class NHibernateReadOnlyScope : IDbReadOnlyScope
    {
        private readonly NHibernateScope _internalScope;

        public NHibernateReadOnlyScope()
            : this(joiningOption: DbScopeOption.JoinExisting, isolationLevel: null)
        { }

        public NHibernateReadOnlyScope(IsolationLevel isolationLevel)
            : this(joiningOption: DbScopeOption.ForceCreateNew, isolationLevel: isolationLevel)
        { }

        public NHibernateReadOnlyScope(DbScopeOption joiningOption, IsolationLevel? isolationLevel)
        {
            _internalScope = new NHibernateScope(
                joiningOption: joiningOption,
                readOnly: true,
                isolationLevel: isolationLevel);
        }

        public void Dispose()
        {
            _internalScope.Dispose();
        }

        public ISession Get<TDbFactory>()
            where TDbFactory : IDbFactory<ISessionFactory>
        {
            return _internalScope.Get<TDbFactory>();
        }
    }
}

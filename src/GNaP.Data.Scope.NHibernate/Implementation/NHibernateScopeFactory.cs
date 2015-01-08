/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

namespace GNaP.Data.Scope.NHibernate.Implementation
{
    using System;
    using System.Data;
    using Interfaces;

    public class NHibernateScopeFactory : IDbScopeFactory
    {
        public IDbScope Create(DbScopeOption joiningOption = DbScopeOption.JoinExisting)
        {
            return new NHibernateScope(
                joiningOption: joiningOption,
                readOnly: false,
                isolationLevel: null);
        }

        public IDbReadOnlyScope CreateReadOnly(DbScopeOption joiningOption = DbScopeOption.JoinExisting)
        {
            return new NHibernateReadOnlyScope(
                joiningOption: joiningOption,
                isolationLevel: null);
        }

        public IDbScope CreateWithTransaction(IsolationLevel isolationLevel)
        {
            return new NHibernateScope(
                joiningOption: DbScopeOption.ForceCreateNew,
                readOnly: false,
                isolationLevel: isolationLevel);
        }

        public IDbReadOnlyScope CreateReadOnlyWithTransaction(IsolationLevel isolationLevel)
        {
            return new NHibernateReadOnlyScope(
                joiningOption: DbScopeOption.ForceCreateNew,
                isolationLevel: isolationLevel);
        }

        public IDisposable SuppressAmbientScope()
        {
            return new AmbientContextSuppressor();
        }
    }
}

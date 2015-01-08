/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

namespace GNaP.Data.Scope.NHibernate.Interfaces
{
    using System;
    using global::NHibernate;

    /// <summary>
    /// A read-only NHibernateScope. Refer to the comments for IDbScope
    /// for more details.
    /// </summary>
    public interface IDbReadOnlyScope : IDisposable
    {
        /// <summary>
        /// Get an ISession instance managed by this NHibernateScope. Don't call SaveChanges() on the ISession itself!
        /// Save the scope instead.
        /// </summary>
        ISession Get<TDbFactory>() where TDbFactory : IDbFactory<ISessionFactory>;
    }
}

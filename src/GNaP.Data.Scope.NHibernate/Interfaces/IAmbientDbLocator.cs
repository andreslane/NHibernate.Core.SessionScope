/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

namespace GNaP.Data.Scope.NHibernate.Interfaces
{
    using global::NHibernate;

    /// <summary>
    /// Convenience methods to retrieve ambient ISession instances.
    /// </summary>
    public interface IAmbientDbLocator
    {
        /// <summary>
        /// If called within the scope of a NHibernateScope, gets or creates
        /// the ambient ISession instance for the provided SessionFactory type.
        ///
        /// Otherwise returns null.
        /// </summary>
        ISession Get<TDbFactory>() where TDbFactory : IDbFactory<ISessionFactory>;
    }
}

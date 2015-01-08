/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

namespace GNaP.Data.Scope.NHibernate.Implementation
{
    using global::NHibernate;
    using Interfaces;

    public class NHibernateAmbientLocator : IAmbientDbLocator
    {
        public ISession Get<TDbFactory>() where TDbFactory : IDbFactory<ISessionFactory>
        {
            var ambientDbScope = NHibernateScope.GetAmbientScope();
            return ambientDbScope == null ? null : ambientDbScope.Get<TDbFactory>();
        }
    }
}

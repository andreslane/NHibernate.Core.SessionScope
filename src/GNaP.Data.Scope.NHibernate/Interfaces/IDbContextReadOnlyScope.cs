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
    /// A read-only DbContextScope. Refer to the comments for IDbContextScope
    /// for more details.
    /// </summary>
    public interface IDbContextReadOnlyScope : IDisposable
    {
        /// <summary>
        /// Get a DbContext instance managed by this DbContextScope. Don't call SaveChanges() on the DbContext itself!
        /// Save the scope instead.
        /// </summary>
        TDbContext Get<TDbContext>() where TDbContext : ISession;
    }
}
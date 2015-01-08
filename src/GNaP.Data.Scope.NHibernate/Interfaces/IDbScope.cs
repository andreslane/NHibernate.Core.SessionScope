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
    using System.Collections;
    using global::NHibernate;

    /// <summary>
    /// Creates and manages the ISession instances used by this code block.
    ///
    /// You typically use a NHibernateScope at the business logic service level. Each
    /// business transaction (i.e. each service method) that uses Entity Framework must
    /// be wrapped in a NHibernateScope, ensuring that the same ISession instances
    /// are used throughout the business transaction and are committed or rolled
    /// back atomically.
    ///
    /// Think of it as TransactionScope but for managing ISession instances instead
    /// of database transactions. Just like a TransactionScope, a NHibernateScope is
    /// ambient, can be nested and supports async execution flows.
    ///
    /// And just like TransactionScope, it does not support parallel execution flows.
    /// You therefore MUST suppress the ambient NHibernateScope before kicking off parallel
    /// tasks or you will end up with multiple threads attempting to use the same ISession
    /// instances (use IDbScopeFactory.SuppressAmbientScope() for this).
    ///
    /// You can access the ISession instances that this scopes manages via either:
    /// - its Get() method, or
    /// - an IAmbientDbContextLocator
    ///
    /// (you would typically use the later in the repository / query layer to allow your repository
    /// or query classes to access the ambient ISession instances without giving them access to the actual
    /// NHibernateScope).
    /// </summary>
    public interface IDbScope : IDisposable
    {
        /// <summary>
        /// Saves the changes in all the ISession instances that were created within this scope.
        /// This method can only be called once per scope.
        /// </summary>
        int SaveChanges();

        /// <summary>
        /// Reloads the provided persistent entities from the data store
        /// in the ISession instances managed by the parent scope.
        ///
        /// If there is no parent scope (i.e. if this NHibernateScope
        /// if the top-level scope), does nothing.
        ///
        /// This is useful when you have forced the creation of a new
        /// NHibernateScope and want to make sure that the parent scope
        /// (if any) is aware of the entities you've modified in the
        /// inner scope.
        ///
        /// (this is a pretty advanced feature that should be used
        /// with parsimony).
        /// </summary>
        void RefreshEntitiesInParentScope(IEnumerable entities);

        /// <summary>
        /// Get an ISession instance managed by this NHibernateScope. Don't call SaveChanges() on the ISession itself!
        /// Save the scope instead.
        /// </summary>
        ISession Get<TDbFactory>() where TDbFactory : IDbFactory<ISessionFactory>;
    }
}

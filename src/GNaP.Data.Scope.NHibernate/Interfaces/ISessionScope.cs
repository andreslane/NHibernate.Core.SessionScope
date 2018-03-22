/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

using System;

namespace NHibernate.SessionScope.Interfaces
{
    /// <summary>
    /// Creates and manages the ISession instances used by this code block.
    ///
    /// You typically use a SessionScope at the business logic service level. Each
    /// business transaction (i.e. each service method) that uses NHibernate must
    /// be wrapped in a SessionScope, ensuring that the same ISession instances
    /// are used throughout the business transaction and are committed or rolled
    /// back atomically.
    ///
    /// Think of it as TransactionScope but for managing ISession instances instead
    /// of database transactions. Just like a TransactionScope, a SessionScope is
    /// ambient, can be nested and supports async execution flows.
    ///
    /// And just like TransactionScope, it does not support parallel execution flows.
    /// You therefore MUST suppress the ambient SessionScope before kicking off parallel
    /// tasks or you will end up with multiple threads attempting to use the same ISession
    /// instances (use ISessionScopeFactory.SuppressAmbientScope() for this).
    ///
    /// You can access the ISession instances that this scopes manages via either:
    /// - its Session property, or
    /// - an IAmbientSessionLocator
    ///
    /// (you would typically use the later in the repository / query layer to allow your repository
    /// or query classes to access the ambient ISession instances without giving them access to the actual
    /// SessionScope).
    /// </summary>
    public interface ISessionScope : IDisposable
    {
        /// <summary>
        /// Saves the changes in all the ISession instances that were created within this scope.
        /// This method can only be called once per scope.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// The ISession instance that this SessionScope manages. Don't save the ISession itself nor commit or rollback the associated ITransaction!
        /// Save the scope instead.
        /// </summary>
        ISession Session { get; }
    }
}

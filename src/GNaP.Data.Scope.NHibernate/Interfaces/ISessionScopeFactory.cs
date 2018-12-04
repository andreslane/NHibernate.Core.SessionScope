/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

using System;
using System.Data;

namespace NHibernate.SessionScope.Interfaces
{
    /// <summary>
    /// Convenience methods to create a new ambient SessionScope. This is the prefered method
    /// to create a SessionScope.
    /// </summary>
    public interface ISessionScopeFactory
    {
        /// <summary>
        /// Creates a new SessionScope.
        /// 
        /// By default, the new scope will join the existing ambient scope. This
        /// is what you want in most cases. This ensures that the same ISession instances
        /// are used by all services methods called within the scope of a business transaction.
        /// 
        /// Set 'joiningOption' to 'ForceCreateNew' if you want to ignore the ambient scope
        /// and force the creation of new ISession instances within that scope. Using 'ForceCreateNew'
        /// is an advanced feature that should be used with great care and only if you fully understand the
        /// implications of doing this.
        /// </summary>
        ISessionScope Create(SessionScopeOption joiningOption = SessionScopeOption.JoinExisting, IInterceptor sessionLocalInterceptor = null);

        /// <summary>
        /// Creates a new SessionScope for read-only queries.
        /// 
        /// By default, the new scope will join the existing ambient scope. This
        /// is what you want in most cases. This ensures that the same ISession instances
        /// are used by all services methods called within the scope of a business transaction.
        /// 
        /// Set 'joiningOption' to 'ForceCreateNew' if you want to ignore the ambient scope
        /// and force the creation of new ISession instances within that scope. Using 'ForceCreateNew'
        /// is an advanced feature that should be used with great care and only if you fully understand the
        /// implications of doing this.
        /// </summary>
        ISessionReadOnlyScope CreateReadOnly(SessionScopeOption joiningOption = SessionScopeOption.JoinExisting, IInterceptor sessionLocalInterceptor = null);

        /// <summary>
        /// Forces the creation of a new ambient SessionScope (i.e. does not
        /// join the ambient scope if there is one) and wraps all ISession instances
        /// created within that scope in an explicit database transaction with 
        /// the provided isolation level. 
        /// 
        /// This is an advanced feature that you should use very carefully
        /// and only if you fully understand the implications of doing this.
        /// </summary>
        ISessionScope CreateWithIsolationLevel(IsolationLevel isolationLevel, IInterceptor sessionLocalInterceptor = null);

        /// <summary>
        /// Forces the creation of a new ambient read-only SessionScope (i.e. does not
        /// join the ambient scope if there is one) and wraps all ISession instances
        /// created within that scope in an explicit database transaction with 
        /// the provided isolation level. 
        /// 
        /// This is an advanced feature that you should use very carefully
        /// and only if you fully understand the implications of doing this.
        /// </summary>
        ISessionReadOnlyScope CreateReadOnlyWithIsolationLevel(IsolationLevel isolationLevel, IInterceptor sessionLocalInterceptor = null);

        /// <summary>
        /// Temporarily suppresses the ambient SessionScope. 
        /// 
        /// Always use this if you need to  kick off parallel tasks within a SessionScope. 
        /// This will prevent the parallel tasks from using the current ambient scope. If you
        /// were to kick off parallel tasks within a SessionScope without suppressing the ambient
        /// context first, all the parallel tasks would end up using the same ambient SessionScope, which 
        /// would result in multiple threads accesssing the same ISession instances at the same 
        /// time.
        /// </summary>
        IDisposable SuppressAmbientScope();
    }
}

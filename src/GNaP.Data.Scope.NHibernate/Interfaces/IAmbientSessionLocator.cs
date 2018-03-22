/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

namespace NHibernate.SessionScope.Interfaces
{
    /// <summary>
    /// Convenience methods to retrieve ambient ISession instances.
    /// </summary>
    public interface IAmbientSessionLocator
    {
        /// <summary>
        /// If called within the scope of a SessionScope, gets or creates
        /// the ambient ISession instance.
        ///
        /// Otherwise returns null.
        /// </summary>
        ISession Get();
    }
}

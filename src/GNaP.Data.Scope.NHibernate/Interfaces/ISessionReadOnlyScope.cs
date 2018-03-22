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
    /// A read-only SessionScope. Refer to the comments for ISessionScope
    /// for more details.
    /// </summary>
    public interface ISessionReadOnlyScope : IDisposable
    {
        /// <summary>
        /// The ISession instance that this SessionScope manages.
        /// </summary>
        ISession Session { get; }
    }
}

/*
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

using NHibernate.SessionScope.Interfaces;

namespace NHibernate.SessionScope
{
    public class AmbientSessionLocator : IAmbientSessionLocator
    {
        public ISession Get()
        {
            SessionScope ambientSessionScope = SessionScope.GetAmbientScope();
            return ambientSessionScope?.Session;
        }
    }
}

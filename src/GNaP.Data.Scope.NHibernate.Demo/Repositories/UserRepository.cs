namespace GNaP.Data.Scope.NHibernate.Demo.Repositories
{
    using System;
    using DomainModel;
    using global::NHibernate;
    using Interfaces;
    using SessionFactories;

    /*
     * An example "repository" relying on an ambient ISession instance.
     *
     * Since we use NHibernate to persist our data, the actual repository is of course the NHibernate ISession. This
     * class is called a "repository" for old time's sake but is merely just a collection
     * of pre-built Linq-to-Entities queries. This avoids having these queries copied and
     * pasted in every service method that need them and facilitates unit testing.
     *
     * Whether your application would benefit from using this additional layer or would
     * be better off if its service methods queried the DbContext directly or used some sort of query
     * object pattern is a design decision for you to make.
     *
     * DbScope is agnostic to this and will happily let you use any approach you
     * deem most suitable for your application.
     *
     */
    public class UserRepository : IUserRepository
    {
        private readonly IAmbientDbLocator _ambientLocator;

        private ISession Session
        {
            get
            {
                var session = _ambientLocator.Get<UserSessionFactory>();

                if (session == null)
                    throw new InvalidOperationException("No ambient ISession of type UserSessionFactory found. This means that this repository method has been called outside of the scope of a DbScope. A repository must only be accessed within the scope of a DbScope, which takes care of creating the ISession instances that the repositories need and making them available as ambient sessions. This is what ensures that, for any given ISession-derived type, the same instance is used throughout the duration of a business transaction. To fix this issue, use IDbScopeFactory in your top-level business logic service method to create a DbScope that wraps the entire business transaction that your service method implements. Then access this repository within that scope. Refer to the comments in the IDbScope.cs file for more details.");

                return session;
            }
        }

        public UserRepository(IAmbientDbLocator ambientLocator)
        {
            if (ambientLocator == null)
                throw new ArgumentNullException("ambientLocator");

            _ambientLocator = ambientLocator;
        }

        public User Get(Guid userId)
        {
            return Session.Get<User>(userId);
        }

        public void Add(User user)
        {
            Session.Save(user);
        }
    }
}

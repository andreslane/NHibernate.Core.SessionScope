using System;
using System.Collections.Generic;
using System.Data;
using NHibernate.Criterion;
using NHibernate.SessionScope.Demo.DomainModel;
using NHibernate.SessionScope.Demo.Repositories;
using NHibernate.SessionScope.Interfaces;

namespace NHibernate.SessionScope.Demo.BusinessLogicServices
{
    /*
     * Example business logic service implementing query functionalities (i.e. read actions).
     */
    public class UserQueryService
    {
        private readonly ISessionScopeFactory _sessionScopeFactory;
        private readonly IUserRepository _userRepository;

        public UserQueryService(ISessionScopeFactory sessionScopeFactory, IUserRepository userRepository)
        {
            _sessionScopeFactory = sessionScopeFactory ?? throw new ArgumentNullException(nameof(sessionScopeFactory));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public User GetUser(Guid userId)
        {
            /*
             * An example of using DbScope for read-only queries.
             * Here, we access the NHibernate ISession directly from
             * the business logic service class.
             *
             * Calling SaveChanges() is not necessary here (and in fact not
             * possible) since we created a read-only scope.
             */
            using (var sessionScope = _sessionScopeFactory.CreateReadOnly())
            {
                var session = sessionScope.Session;
                var user = session.Get<User>(userId);

                if (user == null)
                    throw new ArgumentException(String.Format("Invalid value provided for userId: [{0}]. Couldn't find a user with this ID.", userId));

                return user;
            }
        }

        public IEnumerable<User> GetUsers(params Guid[] userIds)
        {
            using (var sessionScope = _sessionScopeFactory.CreateReadOnly())
            {
                var session = sessionScope.Session;
                return session.CreateCriteria<User>()
                    .Add(Restrictions.In("Id", userIds))
                    .List<User>();
            }
        }

        public User GetUserViaRepository(Guid userId)
        {
            /*
             * Same as GetUsers() but using a repository layer instead of accessing the
             * NHibernate ISession directly.
             *
             * Note how we don't have to worry about knowing what type of ISession the
             * repository will need, about creating the ISession instance or about passing
             * ISession instances around.
             *
             * The SessionScope will take care of creating the necessary ISession instances
             * and making them available as ambient sessions for our repository layer to use.
             * It will also guarantee that only one instance of any given ISession type exists
             * within its scope ensuring that all persistent entities managed within that scope
             * are attached to the same ISession.
             */
            using (_sessionScopeFactory.CreateReadOnly())
            {
                var user = _userRepository.Get(userId);

                if (user == null)
                    throw new ArgumentException(String.Format("Invalid value provided for userId: [{0}]. Couldn't find a user with this ID.", userId));

                return user;
            }
        }

        public User GetUserUncommitted(Guid userId)
        {
            /*
             * An example of explicit database isolation level.
             *
             * Read the comment for CreateReadOnlyWithIsolationLevel() before using this overload
             * as there are gotchas when doing this!
             */
            using (_sessionScopeFactory.CreateReadOnlyWithIsolationLevel(IsolationLevel.ReadUncommitted))
            {
                return _userRepository.Get(userId);
            }
        }
    }
}

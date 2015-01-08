namespace GNaP.Data.Scope.NHibernate.Demo.BusinessLogicServices
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using DomainModel;
    using global::NHibernate.Criterion;
    using Interfaces;
    using Repositories;
    using SessionFactories;

    /*
     * Example business logic service implementing query functionalities (i.e. read actions).
     */
    public class UserQueryService
    {
        private readonly IDbScopeFactory _dbScopeFactory;
        private readonly IUserRepository _userRepository;

        public UserQueryService(IDbScopeFactory dbScopeFactory, IUserRepository userRepository)
        {
            if (dbScopeFactory == null)
                throw new ArgumentNullException("dbScopeFactory");

            if (userRepository == null)
                throw new ArgumentNullException("userRepository");

            _dbScopeFactory = dbScopeFactory;
            _userRepository = userRepository;
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
            using (var dbScope = _dbScopeFactory.CreateReadOnly())
            {
                var session = dbScope.Get<UserSessionFactory>();
                var user = session.Get<User>(userId);

                if (user == null)
                    throw new ArgumentException(String.Format("Invalid value provided for userId: [{0}]. Couldn't find a user with this ID.", userId));

                return user;
            }
        }

        public IEnumerable<User> GetUsers(params Guid[] userIds)
        {
            using (var dbScope = _dbScopeFactory.CreateReadOnly())
            {
                var session = dbScope.Get<UserSessionFactory>();
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
             * The DbScope will take care of creating the necessary ISession instances
             * and making them available as ambient sessions for our repository layer to use.
             * It will also guarantee that only one instance of any given ISession type exists
             * within its scope ensuring that all persistent entities managed within that scope
             * are attached to the same ISession.
             */
            using (_dbScopeFactory.CreateReadOnly())
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
             * An example of explicit database transaction.
             *
             * Read the comment for CreateReadOnlyWithTransaction() before using this overload
             * as there are gotchas when doing this!
             */
            using (_dbScopeFactory.CreateReadOnlyWithTransaction(IsolationLevel.ReadUncommitted))
            {
                return _userRepository.Get(userId);
            }
        }
    }
}

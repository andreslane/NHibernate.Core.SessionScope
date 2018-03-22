using System;
using System.Linq;
using NHibernate.SessionScope.Demo.BusinessLogicServices;
using NHibernate.SessionScope.Demo.CommandModel;
using NHibernate.SessionScope.Demo.Repositories;
using NHibernate.SessionScope.Demo.SessionFactories;
using NHibernate.SessionScope.Interfaces;

namespace NHibernate.SessionScope.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //-- Poor-man DI - build our dependencies by hand for this demo
            ISessionFactory sessionFactory = UserSessionFactory.CreateSessionFactory();
            ISessionScopeFactory sessionScopeFactory = new SessionScopeFactory(sessionFactory);
            IAmbientSessionLocator ambientSessionLocator = new AmbientSessionLocator();
            IUserRepository userRepository = new UserRepository(ambientSessionLocator);

            var userCreationService = new UserCreationService(sessionScopeFactory, userRepository);
            var userQueryService = new UserQueryService(sessionScopeFactory, userRepository);
            var userCreditScoreService = new UserCreditScoreService(sessionScopeFactory);

            try
            {
                Console.WriteLine("This demo application will create a database named NH in the LocalDB SQL Server instance on localhost. Edit the connection string in UserSessionFactory if you'd like to create it somewhere else.");
                Console.WriteLine("Press enter to start...");
                Console.ReadLine();

                //-- Demo of typical usage for read and writes
                Console.WriteLine("Creating a user called Mary...");
                var marySpec = new UserCreationSpec("Mary", "mary@example.com");
                userCreationService.CreateUser(marySpec);
                Console.WriteLine("Done.\n");

                Console.WriteLine("Trying to retrieve our newly created user from the data store...");
                var mary = userQueryService.GetUser(marySpec.Id);
                Console.WriteLine("OK. Persisted user: {0}", mary);

                Console.WriteLine("Trying to retrieve our newly created user from the data store via a repository...");
                var maryViaRepo = userQueryService.GetUserViaRepository(marySpec.Id);
                Console.WriteLine("OK. Persisted user: {0}", maryViaRepo);

                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();

                //-- Demo of nested SessionScopes
                Console.WriteLine("Creating 2 new users called John and Jeanne in an atomic transaction...");
                var johnSpec = new UserCreationSpec("John", "john@example.com");
                var jeanneSpec = new UserCreationSpec("Jeanne", "jeanne@example.com");
                userCreationService.CreateListOfUsers(johnSpec, jeanneSpec);
                Console.WriteLine("Done.\n");

                Console.WriteLine("Trying to retrieve our newly created users from the data store...");
                var createdUsers = userQueryService.GetUsers(johnSpec.Id, jeanneSpec.Id);
                Console.WriteLine("OK. Found {0} persisted users.", createdUsers.Count());

                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();

                //-- Demo of nested SessionScopes in the face of an exception.
                // If any of the provided users failed to get persisted, none should get persisted.
                Console.WriteLine("Creating 2 new users called Julie and Marc in an atomic transaction. Will make the persistence of the second user fail intentionally in order to test the atomicity of the transaction...");
                var julieSpec = new UserCreationSpec("Julie", "julie@example.com");
                var marcSpec = new UserCreationSpec("Marc", "marc@example.com");
                try
                {
                    userCreationService.CreateListOfUsersWithIntentionalFailure(julieSpec, marcSpec);
                    Console.WriteLine("Done.\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                }

                Console.WriteLine("Trying to retrieve our newly created users from the data store...");
                var maybeCreatedUsers = userQueryService.GetUsers(julieSpec.Id, marcSpec.Id);
                Console.WriteLine("Found {0} persisted users. If this number is 0, we're all good. If this number is not 0, we have a big problem.", maybeCreatedUsers.Count());

                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();

                //-- Demo of explicit database transaction isolation level.
                Console.WriteLine("Trying to retrieve user John within a READ UNCOMMITTED database transaction...");
                // You'll want to use SQL Profiler or Entity Framework Profiler to verify that the correct transaction isolation
                // level is being used.
                var userMaybeUncommitted = userQueryService.GetUserUncommitted(johnSpec.Id);
                Console.WriteLine("OK. User found: {0}", userMaybeUncommitted);

                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();

                //-- Demonstration of SessionScope and parallel programming
                Console.WriteLine("Calculating and storing the credit score of all users in the database in parallel...");
                userCreditScoreService.UpdateCreditScoreForAllUsers();
                Console.WriteLine("Done.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
            Console.WriteLine("The end.");
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
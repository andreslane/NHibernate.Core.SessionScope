using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.SessionScope.Demo.DomainModel;
using NHibernate.SessionScope.Interfaces;

namespace NHibernate.SessionScope.Demo.BusinessLogicServices
{
    public class UserCreditScoreService
    {
        private readonly ISessionScopeFactory _sessionScopeFactory;

        public UserCreditScoreService(ISessionScopeFactory sessionScopeFactory)
        {
            _sessionScopeFactory = sessionScopeFactory ?? throw new ArgumentNullException(nameof(sessionScopeFactory));
        }

        public void UpdateCreditScoreForAllUsers()
        {
            /*
             * Demo of SessionScope + parallel programming.
             */

            using (var sessionScope = _sessionScopeFactory.Create())
            {
                //-- Get all users
                var session = sessionScope.Session;
                var userIds = session.CreateCriteria<User>().List<User>().Select(u => u.Id).ToList();

                Console.WriteLine("Found {0} users in the database. Will calculate and store their credit scores in parallel.", userIds.Count);

                //-- Calculate and store the credit score of each user
                // We're going to imagine that calculating a credit score of a user takes some time.
                // So we'll do it in parallel.

                // You MUST call SuppressAmbientScope() when kicking off a parallel execution flow
                // within a SessionScope. Otherwise, this SessionScope will remain the ambient scope
                // in the parallel flows of execution, potentially leading to multiple threads
                // accessing the same ISession instance.
                using (_sessionScopeFactory.SuppressAmbientScope())
                {
                    Parallel.ForEach(userIds, UpdateCreditScore);
                }

                // Note: SaveChanges() isn't going to do anything in this instance since all the changes
                // were actually made and saved in separate DbScopes created in separate threads.
                sessionScope.SaveChanges();
            }
        }

        public void UpdateCreditScore(Guid userId)
        {
            using (var sessionScope = _sessionScopeFactory.Create())
            {
                var session = sessionScope.Session;

                var user = session.Get<User>(userId);
                if (user == null)
                    throw new ArgumentException(String.Format("Invalid userId provided: {0}. Couldn't find a User with this ID.", userId));

                // Simulate the calculation of a credit score taking some time
                var random = new Random(Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(random.Next(300, 1000));

                user.CreditScore = random.Next(1, 100);
                sessionScope.SaveChanges();
            }
        }
    }
}

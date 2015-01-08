namespace GNaP.Data.Scope.NHibernate.Demo.BusinessLogicServices
{
    using System;
    using System.Collections.Generic;
    using DomainModel;
    using Interfaces;
    using SessionFactories;

    public class UserEmailService
    {
        private readonly IDbScopeFactory _dbScopeFactory;

        public UserEmailService(IDbScopeFactory dbScopeFactory)
        {
            if (dbScopeFactory == null)
                throw new ArgumentNullException("dbScopeFactory");

            _dbScopeFactory = dbScopeFactory;
        }

        public void SendWelcomeEmail(Guid userId)
        {
            /*
             * Demo of forcing the creation of a new DbScope
             * to ensure that changes made to the model in this service
             * method are persisted even if that method happens to get
             * called within the scope of a wider business transaction
             * that eventually fails for any reason.
             *
             * This is an advanced feature that should be used as rarely
             * as possible (and ideally, never).
             */

            // We're going to send a welcome email to the provided user
            // (if one hasn't been sent already). Once sent, we'll update
            // that User entity in our DB to record that its Welcome email
            // has been sent.

            // Emails can't be rolled-back. Once they're sent, they're sent.
            // So once the email has been sent successfully, we absolutely
            // must persist this fact in our DB. Even if that method is called
            // by another busines logic service method as part of a wider
            // business transaction and even if that parent business transaction
            // ends up failing for any reason, we still must ensure that
            // we have recorded the fact that the Welcome email has been sent.
            // Otherwise, we would risk spamming our users with repeated Welcome
            // emails.

            // Force the creation of a new DbScope so that the changes we make here are
            // guaranteed to get persisted regardless of what happens after this method has completed.
            using (var dbScope = _dbScopeFactory.Create(DbScopeOption.ForceCreateNew))
            {
                var session = dbScope.Get<UserSessionFactory>();
                var user = session.Get<User>(userId);

                if (user == null)
                    throw new ArgumentException(String.Format("Invalid userId provided: {0}. Couldn't find a User with this ID.", userId));

                if (!user.WelcomeEmailSent)
                {
                    SendEmail(user.Email);
                    user.WelcomeEmailSent = true;
                }

                dbScope.SaveChanges();

                // When you force the creation of a new DbScope, you must force the parent
                // scope (if any) to reload the entities you've modified here. Otherwise, the method calling
                // you might not be able to see the changes you made here.
                dbScope.RefreshEntitiesInParentScope(new List<User> {user});
            }
        }

        private void SendEmail(string emailAddress)
        {
            // Send the email synchronously. Throw if any error occurs.
            // [...]
        }
    }
}

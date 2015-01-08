namespace GNaP.Data.Scope.NHibernate.Demo.Mapping
{
    using FluentNHibernate.Mapping;
    using DomainModel;

    /// <summary>
    /// Defines the convention-based mapping overrides for the User model.
    /// </summary>
    public class UserMapping : ClassMap<User>
    {
        public UserMapping()
        {
            Id(user => user.Id).GeneratedBy.Assigned();
            Map(user => user.Name);
            Map(user => user.CreatedOn);
            Map(user => user.CreditScore);
            Map(user => user.Email);
            Map(user => user.WelcomeEmailSent);
        }
    }
}

using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.SessionScope.Demo.DomainModel;

namespace NHibernate.SessionScope.Demo.Mapping
{
    /// <summary>
    /// Defines the convention-based mapping overrides for the User model.
    /// </summary>
    public class UserMapping : ClassMapping<User>
    {
        public UserMapping()
        {
            Id(user => user.Id, map => map.Generator(Generators.Assigned));
            Property(user => user.Name);
            Property(user => user.CreatedOn);
            Property(user => user.CreditScore);
            Property(user => user.Email);
            Property(user => user.WelcomeEmailSent);
        }
    }
}

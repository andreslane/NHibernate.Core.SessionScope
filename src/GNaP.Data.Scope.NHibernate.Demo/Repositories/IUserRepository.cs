namespace GNaP.Data.Scope.NHibernate.Demo.Repositories
{
    using System;
    using DomainModel;

    public interface IUserRepository
    {
        User Get(Guid userId);
        void Add(User user);
    }
}
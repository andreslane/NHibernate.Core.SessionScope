using System;
using NHibernate.SessionScope.Demo.DomainModel;

namespace NHibernate.SessionScope.Demo.Repositories
{
    public interface IUserRepository
    {
        User Get(Guid userId);
        void Add(User user);
    }
}
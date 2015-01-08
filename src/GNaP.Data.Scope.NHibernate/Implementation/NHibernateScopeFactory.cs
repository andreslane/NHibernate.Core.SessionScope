namespace GNaP.Data.Scope.NHibernate.Implementation
{
    using System;
    using System.Data;
    using global::NHibernate;
    using Interfaces;

    public class NHibernateScopeFactory : IDbContextScopeFactory
    {
        private readonly ISessionFactory _sessionFactory;

        public NHibernateScopeFactory(ISessionFactory sessionFactory = null)
        {
            _sessionFactory = sessionFactory;
        }

        public IDbContextScope Create(DbContextScopeOption joiningOption = DbContextScopeOption.JoinExisting)
        {
            return new NHibernateContextScope(
                joiningOption: joiningOption,
                readOnly: false,
                isolationLevel: null,
                sessionFactory: _sessionFactory);
        }

        public IDbContextReadOnlyScope CreateReadOnly(DbContextScopeOption joiningOption = DbContextScopeOption.JoinExisting)
        {
            return new NHibernateContextReadOnlyScope(
                joiningOption: joiningOption,
                isolationLevel: null,
                sessionFactory: _sessionFactory);
        }

        public IDbContextScope CreateWithTransaction(IsolationLevel isolationLevel)
        {
            return new NHibernateContextScope(
                joiningOption: DbContextScopeOption.ForceCreateNew,
                readOnly: false,
                isolationLevel: isolationLevel,
                sessionFactory: _sessionFactory);
        }

        public IDbContextReadOnlyScope CreateReadOnlyWithTransaction(IsolationLevel isolationLevel)
        {
            return new NHibernateContextReadOnlyScope(
                joiningOption: DbContextScopeOption.ForceCreateNew,
                isolationLevel: isolationLevel,
                sessionFactory: _sessionFactory);
        }

        public IDisposable SuppressAmbientContext()
        {
            return new AmbientContextSuppressor();
        }
    }
}

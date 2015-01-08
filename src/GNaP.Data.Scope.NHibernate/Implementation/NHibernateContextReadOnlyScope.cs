namespace GNaP.Data.Scope.NHibernate.Implementation
{
    using System.Data;
    using global::NHibernate;
    using Interfaces;

    public class NHibernateContextReadOnlyScope : IDbContextReadOnlyScope
    {
        private readonly NHibernateContextScope _internalScope;

        public TDbContext Get<TDbContext>() where TDbContext : ISession
        {
            return _internalScope.Get<TDbContext>();
        }

        public TDbContext Get<TDbContext, TDbContextFactory>() where TDbContext : ISession
        {
            return _internalScope.Get<TDbContext, TDbContextFactory>();
        }

        public ISession GetFromFactory<TDbContextFactory>()
        {
            return _internalScope.GetFromFactory<TDbContextFactory>();
        }

        public NHibernateContextReadOnlyScope(ISessionFactory sessionFactory = null)
            : this(joiningOption: DbContextScopeOption.JoinExisting, isolationLevel: null, sessionFactory: sessionFactory)
        { }

        public NHibernateContextReadOnlyScope(IsolationLevel isolationLevel, ISessionFactory sessionFactory = null)
            : this(joiningOption: DbContextScopeOption.ForceCreateNew, isolationLevel: isolationLevel, sessionFactory: sessionFactory)
        { }

        public NHibernateContextReadOnlyScope(DbContextScopeOption joiningOption, IsolationLevel? isolationLevel, ISessionFactory sessionFactory = null)
        {
            _internalScope = new NHibernateContextScope(joiningOption: joiningOption, readOnly: true, isolationLevel: isolationLevel, sessionFactory: sessionFactory);
        }

        public void Dispose()
        {
            _internalScope.Dispose();
        }
    }
}

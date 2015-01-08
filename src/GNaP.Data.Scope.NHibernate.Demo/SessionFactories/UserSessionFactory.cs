namespace GNaP.Data.Scope.NHibernate.Demo.SessionFactories
{
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using global::NHibernate;
    using global::NHibernate.Context;
    using global::NHibernate.Tool.hbm2ddl;
    using Interfaces;

    public class UserSessionFactory : IDbFactory<ISessionFactory>
    {
        private static ISessionFactory _sessionFactory;

        public ISessionFactory Create()
        {
            return _sessionFactory ?? (_sessionFactory = CreateSessionFactory());
        }

        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                           .Database(MsSqlConfiguration.MsSql2008.ConnectionString(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\nh.mdf;Integrated Security=True"))
                           .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Program>())
                           .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, true)) // TODO: Doesnt this always recreate the database?
                           .CurrentSessionContext<CallSessionContext>()
                           .BuildSessionFactory();
        }
    }
}
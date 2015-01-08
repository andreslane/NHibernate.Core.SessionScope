namespace GNaP.Data.Scope.NHibernate.Demo.SessionFactories
{
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using global::NHibernate;
    using global::NHibernate.Context;
    using global::NHibernate.Tool.hbm2ddl;

    public class UserSessionFactory : IContextFactory<ISessionFactory>
    {
        private const string DbFile = "DemoNHibernate.mdf";
        private static ISessionFactory sessionFactory;

        public ISessionFactory Create()
        {
            return sessionFactory ?? (sessionFactory = CreateSessionFactory());
        }

        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008
                    .ConnectionString(
                        @"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\nh.mdf;Integrated Security=True")
                )
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Program>())
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, true))
                .CurrentSessionContext<CallSessionContext>()
                .BuildSessionFactory();
        }
    }
}
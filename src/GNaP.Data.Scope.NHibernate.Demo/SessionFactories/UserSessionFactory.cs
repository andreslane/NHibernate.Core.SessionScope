using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using NHibernate.SessionScope.Demo.Mapping;
using NHibernate.Tool.hbm2ddl;

namespace NHibernate.SessionScope.Demo.SessionFactories
{
    public static class UserSessionFactory
    {
        public static ISessionFactory CreateSessionFactory()
        {
            var configuration = new Configuration();

            configuration
                .DataBaseIntegration(db =>
                {
                    db.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\nh.mdf;Integrated Security=True";
                    db.Dialect<MsSql2012Dialect>();
                    db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                })
                .AddAssembly(typeof(Program).Assembly)
                .CurrentSessionContext<CallSessionContext>();

            var mapper = new ModelMapper();
            mapper.AddMappings(typeof(UserMapping).Assembly.GetTypes());

            configuration.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

            new SchemaExport(configuration).Create(true, true);

            return configuration.BuildSessionFactory();
        }
    }
}

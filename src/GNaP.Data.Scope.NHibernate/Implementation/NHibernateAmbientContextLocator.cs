namespace GNaP.Data.Scope.NHibernate.Implementation
{
    using global::NHibernate;

    public class NHibernateAmbientContextLocator : Interfaces.IAmbientDbContextLocator
    {


        //public TDbContext Get<TDbContext, TDbContextFactory>() where TDbContext : ISession
        //{
        //    var ambientDbContextScope = NHibernateContextScope.GetAmbientScope();
        //    return ambientDbContextScope == null ? default(TDbContext) : ambientDbContextScope.Get<TDbContext>();
        //}

        //public ISession GetFromFactory<TDbContextFactory>()
        //{
        //    var ambientDbContextScope = NHibernateContextScope.GetAmbientScope();
        //    return ambientDbContextScope == null ? null : ambientDbContextScope.GetFromFactory<TDbContextFactory>();
        //}

        public TDbContext Get<TDbContext>() where TDbContext : ISession
        {
            var ambientDbContextScope = NHibernateContextScope.GetAmbientScope();
            return ambientDbContextScope == null ? default(TDbContext) : ambientDbContextScope.Get<TDbContext>();
        }
    }
}

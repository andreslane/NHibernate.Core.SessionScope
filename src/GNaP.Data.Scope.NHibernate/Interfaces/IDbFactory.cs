namespace GNaP.Data.Scope.NHibernate.Interfaces
{
    public interface IDbFactory<T>
    {
        T Create();
    }
}

namespace CinemaApp.Repositories.IRepositories;

public interface IBulkRepository<T> : IRepository<T> where T : class
{
    bool DeleteRange(IEnumerable<T> entities);
}

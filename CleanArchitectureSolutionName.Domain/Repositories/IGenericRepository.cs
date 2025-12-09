namespace CleanArchitectureSolutionName.Domain.Repositories;
public interface IGenericRepository<T> where T : class
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    void DeleteAsync(T entity);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken);
}

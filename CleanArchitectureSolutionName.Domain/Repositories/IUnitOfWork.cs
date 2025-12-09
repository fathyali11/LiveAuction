namespace CleanArchitectureSolutionName.Domain.Repositories;
public interface IUnitOfWork
{
    //public IRepositoryName RepositoryName { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

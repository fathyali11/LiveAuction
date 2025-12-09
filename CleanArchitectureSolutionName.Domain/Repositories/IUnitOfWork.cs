namespace LiveAuction.Domain.Repositories;
public interface IUnitOfWork
{
    //public IRepositoryName RepositoryName { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;

namespace LiveAuction.Infrastructure.Repositories;
internal class UnitOfWork(ApplicationDbContext _context): IUnitOfWork
{
    private readonly ApplicationDbContext _context = _context;

    //private readonly IRepositoryName ?_repositoryName;
    //public IRepositoryName RepositoryName => _repositoryName ??= new RepositoryName(_context);
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

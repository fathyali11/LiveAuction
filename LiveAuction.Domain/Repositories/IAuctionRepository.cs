using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Repositories;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Auction>> GetAllActiveAsync(CancellationToken cancellationToken);
    Task AddAsync(Auction auction, CancellationToken cancellationToken);
    Task UpdateAsync(Auction auction, CancellationToken cancellationToken);
}

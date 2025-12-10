using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Repositories;

public interface IBidRepository
{
    Task AddAsync(Bid bid, CancellationToken cancellationToken);
    Task<List<Bid>> GetHistoryAsync(int auctionId, CancellationToken cancellationToken);
}

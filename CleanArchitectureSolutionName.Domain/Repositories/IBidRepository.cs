using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Repositories;

public interface IBidRepository
{
    Task AddAsync(Bid bid);
    Task<List<Bid>> GetHistoryAsync(int auctionId);
}

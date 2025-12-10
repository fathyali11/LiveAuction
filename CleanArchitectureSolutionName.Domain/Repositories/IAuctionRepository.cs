using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Repositories;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(int id);
    Task<List<Auction>> GetAllActiveAsync();
    Task AddAsync(Auction auction);
    Task UpdateAsync(Auction auction);
}

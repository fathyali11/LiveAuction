using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Domain.Repositories;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Auction?> GetByIdWithBidsAsync(int id, CancellationToken cancellationToken);
    Task<List<AuctionsInHomePageDto>> GetAllActiveAsync(string? userId, CancellationToken cancellationToken);
    Task AddAsync(Auction auction, CancellationToken cancellationToken);
    Task UpdateAsync(Auction auction, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> AddCurrentBidAsync(int auctionId, decimal amount, CancellationToken cancellationToken);
    Task TerminateAuctionAsync(int auctionId, CancellationToken cancellationToken);
}
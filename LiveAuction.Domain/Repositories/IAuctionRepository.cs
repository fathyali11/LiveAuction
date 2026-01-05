using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Domain.Repositories;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Auction?> GetByIdWithBidsAsync(int id, CancellationToken cancellationToken);
    Task<List<AuctionsInHomePageDto>> GetAllActiveAsync(string? userId, CancellationToken cancellationToken);
    Task AddAsync(Auction auction, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<Auction?> GetAuctionToTerminateAsync(int auctionId, CancellationToken cancellationToken);
}
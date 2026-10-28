using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Domain.Repositories;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Auction?> GetByIdWithBidsAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Read-only projection used for bid validation. Avoids loading/tracking the full auction graph.
    /// </summary>
    Task<AuctionBidContext?> GetBidContextAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Acquires a transaction-scoped SQL Server application lock for the auction so bids and
    /// termination for the same auction are serialized. Must be called inside a transaction.
    /// </summary>
    Task<bool> AcquireAuctionLockAsync(int auctionId, CancellationToken cancellationToken);
    Task<(List<AuctionsInHomePageDto> auctions, int count)> GetAllActiveAndItsCountAsync(string? userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken);
    Task AddAsync(Auction auction, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<Auction?> GetAuctionToTerminateAsync(int auctionId, CancellationToken cancellationToken);
    Task<AuctionUpdateResult?> TryPlaceBidAsync(
    int auctionId,
    string userId,
    decimal amount,
    CancellationToken cancellationToken);

    /// <summary>Persists the Hangfire job id after a post-commit reschedule.</summary>
    Task UpdateJobIdAsync(int auctionId, string jobId, CancellationToken cancellationToken);
}
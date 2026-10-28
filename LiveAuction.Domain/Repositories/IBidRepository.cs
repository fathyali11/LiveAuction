using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Domain.Repositories;

public interface IBidRepository
{
    /// <summary>Adds the bid to the change tracker; the caller owns the unit-of-work save.</summary>
    Task AddAsync(Bid bid, CancellationToken cancellationToken);

    Task<List<Bid>> GetHistoryAsync(int auctionId, CancellationToken cancellationToken);
    Task<List<UserBidDto>> GetUserBidsHistoryAsync(string userId, CancellationToken cancellationToken);

    /// <summary>Returns an existing bid for a replayed idempotency key, if any.</summary>
    Task<BidDto?> GetByIdempotencyKeyAsync(string userId, int auctionId, string idempotencyKey, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

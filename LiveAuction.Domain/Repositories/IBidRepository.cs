using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Domain.Repositories;

public interface IBidRepository
{
    Task AddAsync(Bid bid, CancellationToken cancellationToken);
    Task<List<Bid>> GetHistoryAsync(int auctionId, CancellationToken cancellationToken);
    Task<List<UserBidDto>> GetUserBidsHistoryAsync(string userId, CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

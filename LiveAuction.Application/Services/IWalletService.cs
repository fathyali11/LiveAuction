using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.Services;

public interface IWalletService
{
    Task<(NotificationDto? notification, bool success)> HoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken);
    Task<(NotificationDto? notification, bool success)> DepositAsync(string userId, decimal amount, CancellationToken cancellationToken);
    Task<(NotificationDto? notification, bool success)> ReleaseHoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken);
    Task<decimal> GetActiveHoldAmountAsync(string userId, int auctionId, CancellationToken cancellationToken);
    Task<(List<NotificationDto>? notifications, bool success)> TransferMoneyAsync(string winnerId, string sellerId, decimal amount, int auctionId, CancellationToken cancellationToken);
    Task<PaginatedResult<TransactionResponse>> GetAllTransactionsAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken);
}

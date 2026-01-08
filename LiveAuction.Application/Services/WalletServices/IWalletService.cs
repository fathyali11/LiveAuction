using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.Services.WalletServices;

internal interface IWalletService
{
    Task<bool> HoldAsync(string userId, decimal amount,int auctionId, CancellationToken cancellationToken);
    Task<bool> DepositAsync(string userId, decimal amount, CancellationToken cancellationToken);
    Task<bool> ReleaseHoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken);
    Task<bool> TransferMoneyAsync(string winnerId, string sellerId, decimal amount, int auctionId, CancellationToken cancellationToken);

    Task<PaginatedResult<TransactionResponse>> GetAllTransactionsAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken);
}

namespace LiveAuction.Application.Services.WalletServices;

internal interface IWalletService
{
    Task<bool> HoldAsync(string userId, decimal amount,int auctionId, CancellationToken cancellationToken);
    Task<bool> DepositAsync(string userId, decimal amount, CancellationToken cancellationToken);
    Task<bool> ReleaseHoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken);

}

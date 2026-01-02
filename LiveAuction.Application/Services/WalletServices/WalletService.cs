using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;

namespace LiveAuction.Application.Services.WalletServices;

internal class WalletService(IWalletRepository _walletRepository) : IWalletService
{
    public async Task<bool> HoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken)
    {
        var user = await _walletRepository.GetUserWithBalanceAsync(userId);
        if (user is null || user.AvailableBalance < amount)
            return false;

        user.LockedBalance += amount;

        var transaction = new Transaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = TransactionType.Hold,
            AuctionId = auctionId,
        };

        await _walletRepository.AddTransactionAsync(transaction, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
    
    public async Task<bool> DepositAsync(string userId, decimal amount, CancellationToken cancellationToken)
    {
        if(amount <= 0)
            return false;

        var user = await _walletRepository.GetUserWithBalanceAsync(userId);
        if (user is null)
            return false;

        user.TotalBalance += amount;
        var transaction = new Transaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = TransactionType.Deposit,
        };
        await _walletRepository.AddTransactionAsync(transaction, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReleaseHoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken)
    {
        var user = await _walletRepository.GetUserWithBalanceAsync(userId);
        if (user is null)
            return false;

        user.LockedBalance -= amount;
        var transaction = new Transaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = TransactionType.Release,
            AuctionId = auctionId,
        };
        await _walletRepository.AddTransactionAsync(transaction, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

}

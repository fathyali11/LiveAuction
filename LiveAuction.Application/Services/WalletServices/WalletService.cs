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
    public async Task<bool> TransferMoneyAsync(string winnerId, string sellerId, decimal amount,int auctionId, CancellationToken cancellationToken)
    {
        var winner = await _walletRepository.GetUserWithBalanceAsync(winnerId);
        var seller = await _walletRepository.GetUserWithBalanceAsync(sellerId);
        if (winner is null || seller is null)
            return false;
        winner.LockedBalance -= amount;
        winner.TotalBalance -= amount;
        seller.TotalBalance += amount;
        var transactionForWinner = new Transaction
        {
            UserId = winnerId,
            Amount = amount,
            AuctionId=auctionId,
            TransactionType = TransactionType.Purchase,
        };
        var transactionForSeller = new Transaction
        {
            UserId = sellerId,
            Amount = amount,
            AuctionId=auctionId,
            TransactionType = TransactionType.Deposit,
        };
        await _walletRepository.AddTransactionsAsync([transactionForWinner, transactionForSeller], cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

}

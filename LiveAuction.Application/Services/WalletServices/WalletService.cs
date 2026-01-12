using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services.BackgroundJobServices;
using LiveAuction.Application.Services.NotificationServices;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;

namespace LiveAuction.Application.Services.WalletServices;

internal class WalletService(IWalletRepository _walletRepository,
    IBackgroundJobService _backgroundJobService,
    IAuctionNotificationService _auctionNotificationService) : IWalletService
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
        var notificationDto = new NotificationDto
        {
            Id = transaction.Id,
            UserId = userId,
            Title = "Deposit Successful",
            Message = $"Your deposit of {amount:C} was successful.",
            IsRead = false,
            NotificationType = NotificationType.Wallet.ToString(),
            RelatedEntityId = null,
            CreatedAt = transaction.CreateAt

        };
        _backgroundJobService.EnqueueJob<INotificationService>(
            x=>x.AddNotificationAsync(notificationDto, cancellationToken)
            );
        await _auctionNotificationService.AddNotification(userId, notificationDto);

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

    public async Task<PaginatedResult<TransactionResponse>> GetAllTransactionsAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        paginatedRequest.PageNumber = paginatedRequest.PageNumber <= 0 ? 1 : paginatedRequest.PageNumber;
        paginatedRequest.PageNumber = paginatedRequest.PageNumber <= 0 ? 8 : paginatedRequest.PageNumber;
        var (transactions, count) = await _walletRepository.GetTransactionsAndItsCountAsync(userId, paginatedRequest, cancellationToken);
        return new PaginatedResult<TransactionResponse>
        {
            Items = transactions,
            TotalCount = count,
            PageNumber = paginatedRequest.PageNumber,
            PageSize = paginatedRequest.PageSize
        };
    }
}

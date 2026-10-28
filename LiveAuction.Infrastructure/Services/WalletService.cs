using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Infrastructure.Services;

internal class WalletService(IWalletRepository _walletRepository,
    ILogger<WalletService> _logger) : IWalletService
{
    private const int DefaultPageSize = 8;
    private const int MaxPageSize = 50;

    public async Task<(NotificationDto ?notification, bool success)> HoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
        {
            _logger.LogWarning("Attempted to hold an invalid amount: {Amount} for user {UserId} and auction {AuctionId}.", amount, userId, auctionId);
            return (null, false);
        }
        var isHolded = await _walletRepository.HoldAsync(userId, amount, auctionId, cancellationToken);
        if (!isHolded)
        {
            _logger.LogWarning("Failed to hold amount: {Amount} for user {UserId} and auction {AuctionId}.", amount, userId, auctionId);
            return (null, false);
        }
        var transactionEntity = new Transaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = TransactionType.Hold,
            AuctionId = auctionId,
        };
        await _walletRepository.AddTransactionAsync(transactionEntity, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        var holdNotification = new NotificationDto
        {
            Id = transactionEntity.Id,
            UserId = userId,
            Title = "تم حجز المبلغ بنجاح ⏳",
            Message = $"تم حجز مبلغ {amount:N0} ج.م من محفظتك للمزايدة في المزاد.",
            IsRead = false,
            NotificationType = NotificationType.Wallet.ToString(),
            RelatedEntityId = auctionId,
            CreatedAt = transactionEntity.CreateAt
        };
        return (holdNotification,true);
    }

    public async Task<(NotificationDto ?notification, bool success)> DepositAsync(string userId, decimal amount, CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            _logger.LogWarning("Attempted to deposit a non-positive amount: {Amount} for user {UserId}.", amount, userId);
            return (null, false);
        }
        var isDeposited = await _walletRepository.DepositAsync(userId, amount, cancellationToken);
        if (!isDeposited)
        {
            _logger.LogWarning("Failed to deposit amount: {Amount} for user {UserId}.", amount, userId);
            return (null, false);
        }
        var transactionEntity = new Transaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = TransactionType.Deposit,
        };
        await _walletRepository.AddTransactionAsync(transactionEntity, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        var depositNotification = new NotificationDto
        {
            Id = transactionEntity.Id,
            UserId = userId,
            Title = "تم إيداع المبلغ بنجاح 💰",
            Message = $"تم إيداع مبلغ {amount:N0} ج.م في محفظتك.",
            IsRead = false,
            NotificationType = NotificationType.Wallet.ToString(),
            RelatedEntityId = transactionEntity.Id,
            CreatedAt = transactionEntity.CreateAt
        };
        return (depositNotification, true);
        

    }

    public async Task<(NotificationDto ?notification, bool success)> ReleaseHoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
        {
            _logger.LogWarning("Attempted to release an invalid hold amount: {Amount} for user {UserId} and auction {AuctionId}.", amount, userId, auctionId);
            return (null, false);
        }
        var isReleased = await _walletRepository.ReleaseHoldAsync(userId, amount, auctionId, cancellationToken);
        if (!isReleased)
        {
            _logger.LogWarning("Failed to release hold amount: {Amount} for user {UserId} and auction {AuctionId}.", amount, userId, auctionId);
            return (null, false);
        }
        var transactionEntity = new Transaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = TransactionType.Release,
            AuctionId = auctionId,
        };
        await _walletRepository.AddTransactionAsync(transactionEntity, cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        var releaseNotification = new NotificationDto
        {
            Id = transactionEntity.Id,
            UserId = userId,
            Title = "تم إرجاع المبلغ المحجوز ↩️",
            Message = $"تم إرجاع مبلغ {amount:N0} ج.م من المبالغ المحجوزة إلى رصيدك المتاح.",
            IsRead = false,
            NotificationType = NotificationType.Wallet.ToString(),
            RelatedEntityId = auctionId,
            CreatedAt = transactionEntity.CreateAt
        };
        return (releaseNotification, true);
    }

    public async Task<(List<NotificationDto> ?notifications, bool success)> TransferMoneyAsync(string winnerId, string sellerId, decimal amount, int auctionId, CancellationToken cancellationToken)
    {
        if (sellerId == winnerId)
        {
            _logger.LogWarning("Attempted to transfer money from user {UserId} to themselves for auction {AuctionId}.", winnerId, auctionId);
            return (null, false);
        }
        if (amount <= 0)
        {
            _logger.LogWarning("Attempted to transfer a non-positive amount: {Amount} from user {UserId} to user {OtherUserId} for auction {AuctionId}.", amount, winnerId, sellerId, auctionId);
            return (null, false);
        }

        // Never consume another auction's hold. The winner must have enough locked for this
        // specific auction before we withdraw anything.
        var winnerHold = await _walletRepository.GetActiveHoldAmountAsync(winnerId, auctionId, cancellationToken);
        if (winnerHold < amount)
        {
            _logger.LogWarning(
                "Winner {WinnerId} has only {HoldAmount} held for auction {AuctionId}; {RequiredAmount} is required.",
                winnerId, winnerHold, auctionId, amount);
            return (null, false);
        }

        if (!await _walletRepository.WithdrawFromWinnerAsync(winnerId, amount, cancellationToken))
        {
            _logger.LogWarning("Failed to withdraw amount: {Amount} from user {UserId} for auction {AuctionId}.", amount, winnerId, auctionId);
            return (null, false);
        }
        var isDeposited = await _walletRepository.DepositAsync(sellerId, amount, cancellationToken);
        if (!isDeposited)
        {
            _logger.LogWarning("Failed to deposit amount: {Amount} to user {UserId} for auction {AuctionId}.", amount, sellerId, auctionId);
            await _walletRepository.DepositAsync(winnerId, amount, cancellationToken);
            return (null, false);
        }
        var transactionForWinner = new Transaction
        {
            UserId = winnerId,
            Amount = amount,
            AuctionId = auctionId,
            TransactionType = TransactionType.Purchase,
        };
        var transactionForSeller = new Transaction
        {
            UserId = sellerId,
            Amount = amount,
            AuctionId = auctionId,
            TransactionType = TransactionType.Sale
        };
        // Consume the winner's hold in the ledger as well. Without this row, the hold would
        // appear active forever because LockedBalance alone is not a per-auction ledger.
        var holdConsumedForWinner = new Transaction
        {
            UserId = winnerId,
            Amount = amount,
            AuctionId = auctionId,
            TransactionType = TransactionType.Release
        };
        await _walletRepository.AddTransactionsAsync(
            [transactionForWinner, transactionForSeller, holdConsumedForWinner],
            cancellationToken);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        var winnerNotification = new NotificationDto
        {
            Id = transactionForWinner.Id,
            UserId = winnerId,
            Title = "مبروك! فزت بالمزاد 🎉",
            Message = $"تم خصم مبلغ {amount:N0} ج.م من محفظتك مقابل فوزك بالمزاد.",
            IsRead = false,
            NotificationType = NotificationType.Wallet.ToString(),
            RelatedEntityId = auctionId,
            CreatedAt = transactionForWinner.CreateAt
        };
        var sellerNotification = new NotificationDto
        {
            Id = transactionForSeller.Id,
            UserId = sellerId,
            Title = "تم بيع منتجك بنجاح 🎉",
            Message = $"تم إضافة مبلغ {amount:N0} ج.م إلى محفظتك من بيع المزاد.",
            IsRead = false,
            NotificationType = NotificationType.Wallet.ToString(),
            RelatedEntityId = auctionId,
            CreatedAt = transactionForSeller.CreateAt
        };
        return ([winnerNotification, sellerNotification], true);
    }

    public async Task<PaginatedResult<TransactionResponse>> GetAllTransactionsAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        paginatedRequest.PageNumber = Math.Max(1, paginatedRequest.PageNumber);
        paginatedRequest.PageSize = paginatedRequest.PageSize <= 0
            ? DefaultPageSize
            : Math.Min(paginatedRequest.PageSize, MaxPageSize);

        var (transactions, count) = await _walletRepository.GetTransactionsAndItsCountAsync(userId, paginatedRequest, cancellationToken);
        return new PaginatedResult<TransactionResponse>
        {
            Items = transactions,
            TotalCount = count,
            PageNumber = paginatedRequest.PageNumber,
            PageSize = paginatedRequest.PageSize
        };
    }

    public async Task<decimal> GetActiveHoldAmountAsync(string userId, int auctionId, CancellationToken cancellationToken)
    {
        return await _walletRepository.GetActiveHoldAmountAsync(userId, auctionId, cancellationToken);
    }
}

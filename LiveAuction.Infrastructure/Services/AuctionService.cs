using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Infrastructure.Services;

internal class AuctionService(IBackgroundJobService _backgroundJobService,
    IAuctionNotificationService _auctionNotificationService,
    IAuctionRepository _auctionRepository,
    ILogger<AuctionService> _logger,
    IServiceProvider _serviceProvider) : IAuctionService
{
    public async Task<PaginatedResult<AuctionsInHomePageDto>> GetAllActiveAuctionsAsync(string? userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        paginatedRequest.PageNumber = paginatedRequest.PageNumber <= 0 ? 1 : paginatedRequest.PageNumber;
        paginatedRequest.PageSize = paginatedRequest.PageSize <= 0 ? 8 : paginatedRequest.PageSize;
        var (auctions, count) = await _auctionRepository.GetAllActiveAndItsCountAsync(userId, paginatedRequest, cancellationToken);
        return new PaginatedResult<AuctionsInHomePageDto>
        {
            Items = auctions,
            TotalCount = count,
            PageNumber = paginatedRequest.PageNumber,
            PageSize = paginatedRequest.PageSize
        };
    }

    public Task<string> ScheduleAuction(int auctionId, DateTime endTime, CancellationToken cancellationToken)
    {
        var delay = endTime - DateTime.UtcNow;

        if (delay < TimeSpan.FromSeconds(10))
        {
            delay = TimeSpan.FromSeconds(10);
        }

        // Do not capture the caller's cancellation token in a job that runs much later:
        // it will already be cancelled or disposed by then.
        var jobId = _backgroundJobService.ScheduleJob<IAuctionService>(
            service => service.TerminateAuctionAsync(auctionId, CancellationToken.None),
            delay);

        return Task.FromResult(jobId);
    }

    public Task CancelScheduledAuction(string jobId, CancellationToken cancellationToken)
    {
        _backgroundJobService.DeleteScheduledJob(jobId);
        return Task.CompletedTask;
    }

    public async Task TerminateAuctionAsync(int auctionId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        await using var transaction = await auctionRepository.BeginTransactionAsync(cancellationToken);
        var notifications = new List<NotificationDto>();
        string? winnerId = null;
        string? sellerId = null;

        try
        {
            // Serialize termination with bids for the same auction.
            if (!await auctionRepository.AcquireAuctionLockAsync(auctionId, cancellationToken))
            {
                throw new InvalidOperationException($"Could not acquire the auction lock for auction {auctionId}.");
            }

            var auction = await auctionRepository.GetAuctionToTerminateAsync(auctionId, cancellationToken);
            if (auction is null || auction.Status != AuctionStatus.Open)
            {
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            // A late bid may have extended the auction after this job was scheduled.
            // Reschedule instead of closing too early.
            if (DateTime.UtcNow < auction.EndTime)
            {
                auction.JobId = await ScheduleAuction(auction.Id, auction.EndTime, cancellationToken);
                await auctionRepository.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation(
                    "Auction {AuctionId} was extended; termination rescheduled for {EndTime}.",
                    auction.Id, auction.EndTime);
                return;
            }

            auction.Status = AuctionStatus.Closed;

            if (auction.Bids.Count > 0)
            {
                var highestBid = auction.Bids.OrderByDescending(b => b.Amount).First();
                auction.WinnerId = highestBid.BidderId;
                auction.CurrentBidderId = highestBid.BidderId;
                winnerId = highestBid.BidderId;
                sellerId = auction.CreatedById;

                var transferResult = await walletService.TransferMoneyAsync(
                    highestBid.BidderId,
                    auction.CreatedById,
                    highestBid.Amount,
                    auctionId,
                    cancellationToken);

                if (!transferResult.success)
                {
                    throw new InvalidOperationException(
                        $"Money transfer failed for auction {auctionId} and winner {highestBid.BidderId}.");
                }

                if (transferResult.notifications is not null)
                {
                    notifications.AddRange(transferResult.notifications);
                }

                // Safety net: a previous failure may have left a losing bidder's hold behind.
                var losingBidderIds = auction.Bids
                    .Where(b => b.BidderId != highestBid.BidderId)
                    .Select(b => b.BidderId)
                    .Distinct(StringComparer.Ordinal);

                foreach (var losingBidderId in losingBidderIds)
                {
                    var activeHold = await walletService.GetActiveHoldAmountAsync(losingBidderId, auctionId, cancellationToken);
                    if (activeHold <= 0)
                    {
                        continue;
                    }

                    var releaseResult = await walletService.ReleaseHoldAsync(
                        losingBidderId, activeHold, auctionId, cancellationToken);

                    if (releaseResult.success && releaseResult.notification is not null)
                    {
                        notifications.Add(releaseResult.notification);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to release the remaining hold for user {UserId} on auction {AuctionId}.",
                            losingBidderId, auctionId);
                    }
                }
            }

            await auctionRepository.SaveChangesAsync(cancellationToken);

            var notificationEntities = notifications.Select(ToNotificationEntity).ToList();
            if (notificationEntities.Count > 0)
            {
                await notificationRepository.AddRangeAsync(notificationEntities, cancellationToken);
                await auctionRepository.SaveChangesAsync(cancellationToken);

                for (var i = 0; i < notifications.Count; i++)
                {
                    notifications[i].Id = notificationEntities[i].Id;
                    notifications[i].CreatedAt = notificationEntities[i].CreatedAt;
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackException)
            {
                _logger.LogError(rollbackException, "Failed to roll back termination of auction {AuctionId}.", auctionId);
            }

            throw;
        }

        foreach (var notification in notifications)
        {
            try
            {
                await _auctionNotificationService.AddNotification(notification.UserId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to broadcast termination notification for user {UserId} on auction {AuctionId}.",
                    notification.UserId, auctionId);
            }
        }

        if (winnerId is not null)
        {
            await TryRefreshWalletAsync(winnerId, auctionId);
            await TryRefreshWalletAsync(sellerId!, auctionId);
        }
    }

    private async Task TryRefreshWalletAsync(string userId, int auctionId)
    {
        try
        {
            await _auctionNotificationService.ForceRefreshWalletAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to refresh the wallet for user {UserId} after auction {AuctionId} ended.",
                userId, auctionId);
        }
    }

    private static Notification ToNotificationEntity(NotificationDto dto)
        => new()
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            IsRead = false,
            NotificationType = Enum.Parse<NotificationType>(dto.NotificationType),
            RelatedEntityId = dto.RelatedEntityId,
            CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt
        };
}

using LiveAuction.Application.Bids.Commands.CreateBid;
using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Infrastructure.Services;

internal class BidService(
    IBidRepository _bidRepository,
    ILogger<BidService> _logger,
    IAuctionNotificationService _auctionNotificationService,
    UserManager<ApplicationUser> _userManager,
    IAuctionService _auctionService,
    IBackgroundJobService _backgroundJobService,
    IAuctionRepository _auctionRepository,
    IUserService _userService,
    IWalletService _walletService,
    INotificationRepository _notificationRepository) : IBidService
{
    public async Task<OneOf<Error, BidDto>> AddBidAsync(CreateBidCommand request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            _logger.LogWarning("Cannot create a bid without an authenticated user.");
            return new Error(ErrorCodes.UnauthorizedError, "You must be signed in to place a bid.");
        }

        var idempotencyKey = request.IdempotencyKey?.Trim();
        if (string.IsNullOrWhiteSpace(idempotencyKey) ||
            idempotencyKey.Length > BidRules.IdempotencyKeyMaxLength)
        {
            _logger.LogWarning(
                "Bid rejected because the idempotency key is missing or too long. UserId: {UserId}, AuctionId: {AuctionId}.",
                currentUserId, request.AuctionId);
            return new Error(ErrorCodes.ValidationError, "A valid idempotency key is required.");
        }

        // Replays must return the original result without touching the wallet or the auction.
        var existingBid = await _bidRepository.GetByIdempotencyKeyAsync(
            currentUserId, request.AuctionId, idempotencyKey, cancellationToken);
        if (existingBid is not null)
        {
            if (existingBid.Amount != request.Amount)
            {
                _logger.LogWarning(
                    "Idempotency key was reused with a different amount. UserId: {UserId}, AuctionId: {AuctionId}.",
                    currentUserId, request.AuctionId);
                return new Error(ErrorCodes.Conflict, "The idempotency key was already used for a different amount.");
            }

            _logger.LogInformation(
                "Returning the existing bid for replayed idempotency key. UserId: {UserId}, AuctionId: {AuctionId}.",
                currentUserId, request.AuctionId);
            return existingBid;
        }

        if (request.Amount <= 0 ||
            request.Amount > BidRules.MaximumAmount ||
            decimal.Round(request.Amount, 2) != request.Amount)
        {
            _logger.LogWarning(
                "Bid rejected because the amount is invalid. UserId: {UserId}, AuctionId: {AuctionId}, Amount: {Amount}.",
                currentUserId, request.AuctionId, request.Amount);
            return new Error(ErrorCodes.ValidationError, "The bid amount is invalid.");
        }

        var bidder = await _userManager.FindByIdAsync(currentUserId);
        if (bidder is null)
        {
            _logger.LogWarning("User with Id {UserId} not found while placing a bid.", currentUserId);
            return new Error(ErrorCodes.NotFoundError, "User not found.");
        }

        if (bidder.LockoutEnd is { } lockoutEnd && lockoutEnd > DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Locked-out user {UserId} attempted to place a bid.", currentUserId);
            return new Error(ErrorCodes.ForbiddenError, "Your account is not allowed to place bids right now.");
        }

        var auction = await _auctionRepository.GetBidContextAsync(request.AuctionId, cancellationToken);
        if (auction is null)
        {
            _logger.LogWarning("Auction with Id {AuctionId} not found while placing a bid.", request.AuctionId);
            return new Error(ErrorCodes.NotFoundError, "Auction not found.");
        }

        if (auction.CreatedById == currentUserId)
        {
            _logger.LogWarning(
                "User with Id {UserId} attempted to bid on their own auction {AuctionId}.",
                currentUserId, request.AuctionId);
            return new Error(ErrorCodes.ValidationError, "لا يمكنك المزايدة على مزادك الخاص.");
        }

        var now = DateTime.UtcNow;
        if (auction.Status != AuctionStatus.Open || now < auction.StartTime || now >= auction.EndTime)
        {
            _logger.LogWarning(
                "Auction {AuctionId} is not open for bidding. Status: {Status}, StartTime: {StartTime}, EndTime: {EndTime}.",
                auction.Id, auction.Status, auction.StartTime, auction.EndTime);
            return new Error(ErrorCodes.ValidationError, "المزاد غير مفتوح للمزايدة حالياً.");
        }

        var minimumBid = auction.CurrentBid + BidRules.MinimumIncrement;
        if (request.Amount < minimumBid)
        {
            _logger.LogWarning(
                "Bid amount {Amount} is below the minimum {MinimumBid} for auction {AuctionId}.",
                request.Amount, minimumBid, auction.Id);
            return new Error(
                ErrorCodes.ValidationError,
                $"يجب أن تكون المزايدة {BidRules.MinimumIncrement:N0} ج.م على الأقل أعلى من السعر الحالي.");
        }

        BidDto bidDto = null!;
        AuctionUpdateResult updateResult = null!;
        var notifications = new List<NotificationDto>();

        await using var transaction = await _bidRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            // Serialize all bids for this auction. The lock is released when the transaction ends,
            // so two concurrent bids can no longer release each other's holds.
            if (!await _auctionRepository.AcquireAuctionLockAsync(auction.Id, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new Error(ErrorCodes.Conflict, "المزاد مشغول حالياً. حاول مرة أخرى.");
            }

            // Do the authoritative update first: it re-checks status/time/price and returns the
            // previous bidder and amount from the row as it was before the update.
            var update = await _auctionRepository.TryPlaceBidAsync(
                auction.Id, currentUserId, request.Amount, cancellationToken);

            if (update is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogInformation(
                    "Auction {AuctionId} changed before the bid could be applied. UserId: {UserId}.",
                    auction.Id, currentUserId);
                return new Error(ErrorCodes.Conflict, "تغيّر سعر المزاد. حدّث الصفحة وحاول مرة أخرى.");
            }

            updateResult = update;

            // The bidder may already be the highest bidder (raising their own bid). In that case
            // only the difference needs to be held; the previous hold must NOT be released.
            var activeHold = await _walletService.GetActiveHoldAmountAsync(
                currentUserId, auction.Id, cancellationToken);

            var additionalHold = request.Amount - activeHold;
            if (additionalHold > 0)
            {
                var holdResult = await _walletService.HoldAsync(
                    currentUserId, additionalHold, auction.Id, cancellationToken);

                if (!holdResult.success)
                {
                    await SafeRollbackAsync(transaction);
                    _logger.LogWarning(
                        "Failed to hold {Amount} for user {UserId} on auction {AuctionId}.",
                        additionalHold, currentUserId, auction.Id);
                    return new Error(ErrorCodes.ValidationError, "لا يوجد لديك مال كافٍ. اشحن محفظتك ثم حاول مرة أخرى.");
                }

                if (holdResult.notification is not null)
                {
                    notifications.Add(holdResult.notification);
                }
            }
            else if (additionalHold < 0)
            {
                // Defensive: should not happen because a new bid is always higher, but never
                // leave the wallet over-locked if the ledger is inconsistent.
                var releaseExcess = await _walletService.ReleaseHoldAsync(
                    currentUserId, -additionalHold, auction.Id, cancellationToken);
                if (releaseExcess.success && releaseExcess.notification is not null)
                {
                    notifications.Add(releaseExcess.notification);
                }
            }

            // Release exactly what the previous bidder has held on this auction. Their hold may
            // already be zero (for example after a retry), in which case there is nothing to do.
            var previousBidderId = updateResult.PreviousBidderId;
            if (!string.IsNullOrWhiteSpace(previousBidderId) &&
                !string.Equals(previousBidderId, currentUserId, StringComparison.Ordinal))
            {
                var previousHold = await _walletService.GetActiveHoldAmountAsync(
                    previousBidderId, auction.Id, cancellationToken);

                if (previousHold > 0)
                {
                    var releaseResult = await _walletService.ReleaseHoldAsync(
                        previousBidderId, previousHold, auction.Id, cancellationToken);

                    if (!releaseResult.success)
                    {
                        await SafeRollbackAsync(transaction);
                        _logger.LogError(
                            "Failed to release hold {Amount} for previous bidder {UserId} on auction {AuctionId}.",
                            previousHold, previousBidderId, auction.Id);
                        return new Error(ErrorCodes.InternalServerError, "تعذر إتمام المزايدة. حاول مرة أخرى.");
                    }

                    if (releaseResult.notification is not null)
                    {
                        notifications.Add(releaseResult.notification);
                    }
                }
            }

            var bidTime = DateTime.UtcNow;
            var bid = new Bid
            {
                AuctionId = auction.Id,
                Amount = request.Amount,
                BidderId = currentUserId,
                BidTime = bidTime,
                IdempotencyKey = idempotencyKey
            };

            await _bidRepository.AddAsync(bid, cancellationToken);

            notifications.Add(new NotificationDto
            {
                UserId = currentUserId,
                Title = "تمت المزايدة بنجاح ✅",
                Message = $"تم تسجيل مزايدتك بقيمة {request.Amount:N0} ج.م على المزاد '{auction.Title}' بنجاح.",
                IsRead = false,
                NotificationType = NotificationType.Auction.ToString(),
                RelatedEntityId = auction.Id
            });

            if (!string.IsNullOrWhiteSpace(previousBidderId) &&
                !string.Equals(previousBidderId, currentUserId, StringComparison.Ordinal))
            {
                notifications.Add(new NotificationDto
                {
                    UserId = previousBidderId,
                    Title = "عاجل: تم تخطي مزايدتك ⚠️",
                    Message = $"قام شخص آخر بتقديم عرض أعلى منك على '{auction.Title}'. السعر الحالي أصبح {request.Amount:N0} ج.م. سارع بالمزايدة الآن لاستعادة الصدارة!",
                    IsRead = false,
                    NotificationType = NotificationType.Auction.ToString(),
                    RelatedEntityId = auction.Id
                });
            }

            // Persist notifications inside the same transaction as the bid. If the bid is
            // rolled back, so are its notifications; if it commits, they are durable.
            var notificationEntities = notifications.Select(ToNotificationEntity).ToList();
            if (notificationEntities.Count > 0)
            {
                await _notificationRepository.AddRangeAsync(notificationEntities, cancellationToken);
            }

            await _bidRepository.SaveChangesAsync(cancellationToken);

            // Copy the generated ids back so the realtime push matches the stored rows.
            for (var i = 0; i < notifications.Count; i++)
            {
                notifications[i].Id = notificationEntities[i].Id;
                notifications[i].CreatedAt = notificationEntities[i].CreatedAt;
            }

            await transaction.CommitAsync(cancellationToken);

            bidDto = new BidDto
            {
                Id = bid.Id,
                AuctionId = bid.AuctionId,
                Amount = bid.Amount,
                AuctionEndTime = updateResult.EndTime,
                Bidder = bidder.FullName,
                TimePlaced = bid.BidTime
            };
        }
        catch (DbUpdateException dbEx) when (IsIdempotencyConflict(dbEx))
        {
            await SafeRollbackAsync(transaction);
            _logger.LogWarning(
                "Concurrent duplicate bid detected for auction {AuctionId}, user {UserId}.",
                request.AuctionId, currentUserId);

            var replayedBid = await _bidRepository.GetByIdempotencyKeyAsync(
                currentUserId, request.AuctionId, idempotencyKey, cancellationToken);

            if (replayedBid is null)
            {
                return new Error(ErrorCodes.Duplicate, "This bid has already been processed.");
            }

            return replayedBid.Amount == request.Amount
                ? replayedBid
                : new Error(ErrorCodes.Conflict, "The idempotency key was already used for a different amount.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await SafeRollbackAsync(transaction);
            throw;
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(transaction);
            _logger.LogError(ex,
                "Error occurred while creating bid for AuctionId: {AuctionId}, UserId: {UserId}.",
                request.AuctionId, currentUserId);
            return new Error(ErrorCodes.InternalServerError, "تعذر إتمام المزايدة. حاول مرة أخرى.");
        }

        // Everything below runs after the transaction committed and must never change the
        // outcome of the bid. Failures are logged, not returned to the caller.
        await RescheduleIfExtendedAsync(auction, updateResult);
        await BroadcastBidAsync(updateResult.Id, bidDto);
        await BroadcastNotificationsAsync(notifications);
        await RefreshWalletsAsync(currentUserId, updateResult.PreviousBidderId);

        return bidDto;
    }

    private async Task RescheduleIfExtendedAsync(AuctionBidContext auction, AuctionUpdateResult updateResult)
    {
        var wasExtended = updateResult.EndTime > auction.EndTime ||
                          updateResult.ExtensionCount > auction.ExtensionCount;

        if (!wasExtended)
        {
            return;
        }

        try
        {
            // Schedule the replacement first: if scheduling fails, the old job still exists and
            // will re-check EndTime when it runs.
            var newJobId = await _auctionService.ScheduleAuction(
                updateResult.Id, updateResult.EndTime, CancellationToken.None);

            if (!string.IsNullOrWhiteSpace(auction.JobId))
            {
                try
                {
                    _backgroundJobService.DeleteScheduledJob(auction.JobId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to delete the previous termination job {JobId} for auction {AuctionId}.",
                        auction.JobId, auction.Id);
                }
            }

            await _auctionRepository.UpdateJobIdAsync(updateResult.Id, newJobId, CancellationToken.None);

            _logger.LogInformation(
                "Auction {AuctionId} extended to {EndTime}; termination rescheduled with job {JobId}.",
                auction.Id, updateResult.EndTime, newJobId);
        }
        catch (Exception ex)
        {
            // The old termination job will run at the old EndTime and reschedule itself because
            // TerminateAuctionAsync re-checks EndTime before closing.
            _logger.LogError(ex,
                "Failed to reschedule the termination job for extended auction {AuctionId}.",
                auction.Id);
        }
    }

    private async Task BroadcastBidAsync(int auctionId, BidDto bidDto)
    {
        try
        {
            await _auctionNotificationService.NotifyNewBidAsync(auctionId, bidDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast the new bid for AuctionId: {AuctionId}.", auctionId);
        }
    }

    private async Task BroadcastNotificationsAsync(IReadOnlyCollection<NotificationDto> notifications)
    {
        foreach (var notification in notifications)
        {
            try
            {
                await _auctionNotificationService.AddNotification(notification.UserId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to broadcast notification {NotificationTitle} to user {UserId}.",
                    notification.Title, notification.UserId);
            }
        }
    }

    private async Task RefreshWalletsAsync(string currentUserId, string? previousBidderId)
    {
        await TryRefreshWalletAsync(currentUserId);

        if (!string.IsNullOrWhiteSpace(previousBidderId) &&
            !string.Equals(previousBidderId, currentUserId, StringComparison.Ordinal))
        {
            await TryRefreshWalletAsync(previousBidderId);
        }
    }

    private async Task TryRefreshWalletAsync(string userId)
    {
        try
        {
            await _auctionNotificationService.ForceRefreshWalletAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh the wallet for user {UserId}.", userId);
        }
    }

    private async Task SafeRollbackAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to roll back the bid transaction.");
        }
    }

    private static bool IsIdempotencyConflict(DbUpdateException exception)
    {
        if (exception.GetBaseException() is not SqlException sqlException)
        {
            return false;
        }

        return sqlException.Number is 2601 or 2627 &&
               sqlException.Message.Contains("IdempotencyKey", StringComparison.OrdinalIgnoreCase);
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

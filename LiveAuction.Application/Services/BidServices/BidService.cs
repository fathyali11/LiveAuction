using LiveAuction.Application.Bids.Commands.CreateBid;
using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services.AuctionServices;
using LiveAuction.Application.Services.BackgroundJobServices;
using LiveAuction.Application.Services.NotificationServices;
using LiveAuction.Application.Services.WalletServices;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OneOf;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Services.BidServices;

internal class BidService(
    IBidRepository _bidRepository,
    ILogger<BidService> _logger,
    IAuctionNotificationService _auctionNotificationService,
    UserManager<ApplicationUser> _userManager,
    IAuctionService _auctionService,
    IBackgroundJobService _backgroundJobService,
    IAuctionRepository _auctionRepository,
    IWalletService _walletService) : IBidService
{
    public async Task<OneOf<Error, BidDto>> AddBidAsync(CreateBidCommand request, CancellationToken cancellationToken = default)
    {
        using var transaction = await _bidRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var bidder = await _userManager.FindByIdAsync(request.UserId);
            if (bidder == null)
            {
                var errorMessage = $"User with Id {request.UserId} not found.";
                _logger.LogWarning(errorMessage);
                return new Error(ErrorCodes.NotFoundError, errorMessage);
            }

            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction == null)
            {
                var errorMessage = $"Auction with Id {request.AuctionId} not found.";
                _logger.LogWarning(errorMessage);
                return new Error(ErrorCodes.NotFoundError, errorMessage);
            }

            if (request.Amount <= auction.CurrentBid)
            {
                var errorMessage = $"Bid amount must be higher than the current bid of {auction.CurrentBid}.";
                _logger.LogWarning(errorMessage);
                return new Error(ErrorCodes.ValidationError, errorMessage);
            }

            var IsHolded = await _walletService.HoldAsync(request.UserId, request.Amount, request.AuctionId, cancellationToken);
            if (!IsHolded)
            {
                var errorMessage = $"لا يوجدك لديك مال كافي اشحن محفظتك";
                _logger.LogWarning(errorMessage);
                return new Error(ErrorCodes.ValidationError, errorMessage);
            }

            var lastBidderId = auction.CurrentBidderId;

            if (!string.IsNullOrEmpty(lastBidderId))
            {
                var IsReleased = await _walletService.ReleaseHoldAsync(lastBidderId!, auction.CurrentBid, auction.Id, cancellationToken);
                if (!IsReleased)
                {
                    var errorMessage = $"Failed to release money for previous highest bidder";
                    _logger.LogWarning(errorMessage);
                    return new Error(ErrorCodes.InternalServerError, errorMessage);
                }
            }

            if (auction.EndTime - DateTime.UtcNow <= TimeSpan.FromMinutes(5))
            {
                auction.EndTime = auction.EndTime.AddMinutes(1);
                _backgroundJobService.DeleteScheduledJob(auction.JobId);
                auction.JobId = await _auctionService.ScheduleAuction(auction, cancellationToken);
                _logger.LogInformation("AuctionId: {AuctionId} end time extended to {EndTime}", auction.Id, auction.EndTime);
            }
            auction.CurrentBid = request.Amount;
            auction.CurrentBidderId = request.UserId;
            await _auctionRepository.SaveChangesAsync(cancellationToken);
            var bid = request.Adapt<Bid>();

            await _bidRepository.AddAsync(bid, cancellationToken);
            _logger.LogInformation("Bid created with Id: {BidId} for AuctionId: {AuctionId}", bid.Id, request.AuctionId);
            var bidDto = bid.Adapt<BidDto>();
            bidDto.AuctionEndTime = auction.EndTime;
            await transaction.CommitAsync(cancellationToken);

            var createNotificateDtoToBidder = new NotificationDto
            {
                UserId = request.UserId,
                Title = "تمت المزايدة بنجاح ✅",
                Message = $"تم تسجيل مزايدتك بقيمة {request.Amount:N0} ج.م على المزاد '{auction.Title}' بنجاح.",
                IsRead = false,
                NotificationType = NotificationType.Auction.ToString(),
                RelatedEntityId = request.AuctionId
            };
            _backgroundJobService.EnqueueJob<INotificationService>(
                x => x.AddNotificationAsync(createNotificateDtoToBidder));
            await _auctionNotificationService.NotifyNewBidAsync(bid.AuctionId, bidDto);
            await _auctionNotificationService.AddNotification(request.UserId, createNotificateDtoToBidder);

            if(!string.IsNullOrEmpty(lastBidderId) && lastBidderId != request.UserId)
            {
                var createNotificateDtoToLastBidder = new NotificationDto
                {
                    UserId = lastBidderId!,
                    Title = "عاجل: تم تخطي مزايدتك ⚠️",
                    Message = $"قام شخص آخر بتقديم عرض أعلى منك على '{auction.Title}'. السعر الحالي أصبح {request.Amount:N0} ج.م. سارع بالمزايدة الآن لاستعادة الصدارة!",
                    IsRead = false,
                    NotificationType = NotificationType.Auction.ToString(),
                    RelatedEntityId = request.AuctionId
                };
                _backgroundJobService.EnqueueJob<INotificationService>(
                    x => x.AddNotificationAsync(createNotificateDtoToLastBidder));
                await _auctionNotificationService.AddNotification(lastBidderId!, createNotificateDtoToLastBidder);
            }
            return bidDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating bid for AuctionId: {AuctionId}, UserId: {UserId}",
                request.AuctionId, request.UserId);
            await transaction.RollbackAsync(cancellationToken);
            return new Error(ErrorCodes.InternalServerError, "An error occurred while processing your bid.");
        }

    }
}

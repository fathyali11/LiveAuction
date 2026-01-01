using FluentValidation;
using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services.AuctionServices;
using LiveAuction.Application.Services.BackgroundJobServices;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Bids.Commands.CreateBid;

internal class CreateBidCommandHandler(IBidRepository bidRepository,
    ILogger<CreateBidCommandHandler> logger,
    IValidator<CreateBidCommand> validator,
    IAuctionNotificationService auctionNotificationService,
    UserManager<ApplicationUser> userManager,
    IAuctionService _auctionService,
    IBackgroundJobService _backgroundJobService,
    IAuctionRepository _auctionRepository) : IRequestHandler<CreateBidCommand, OneOf<Error, BidDto>>
{
    public async Task<OneOf<Error, BidDto>> Handle(CreateBidCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateBidCommand for AuctionId: {AuctionId}, UserId: {UserId}, Amount: {Amount}",
            request.AuctionId, request.UserId, request.Amount);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            logger.LogWarning("Validation failed for CreateBidCommand: {Errors}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var bidder = await userManager.FindByIdAsync(request.UserId);
        if (bidder == null)
        {
            var errorMessage = $"User with Id {request.UserId} not found.";
            logger.LogWarning(errorMessage);
            return new Error(ErrorCodes.NotFoundError, errorMessage);
        }
        var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
        if (auction == null)
        {
            var errorMessage = $"Auction with Id {request.AuctionId} not found.";
            logger.LogWarning(errorMessage);
            return new Error(ErrorCodes.NotFoundError, errorMessage);
        }
        if(auction.EndTime-DateTime.UtcNow <= TimeSpan.FromMinutes(5))
        {
            auction.EndTime = auction.EndTime.AddMinutes(5);
            _backgroundJobService.DeleteScheduledJob(auction.JobId);
            auction.JobId = await _auctionService.ScheduleAuction(auction,cancellationToken);
            logger.LogInformation("AuctionId: {AuctionId} end time extended to {EndTime}", auction.Id, auction.EndTime);
        }
        auction.CurrentBid = request.Amount;
        auction.CurrentBidderId = request.UserId;
        await _auctionRepository.UpdateAsync(auction, cancellationToken);
        var bid = request.Adapt<Bid>();
        
        await bidRepository.AddAsync(bid, cancellationToken);
        logger.LogInformation("Bid created with Id: {BidId} for AuctionId: {AuctionId}", bid.Id, request.AuctionId);
        var bidDto = bid.Adapt<BidDto>();
        bidDto.AuctionEndTime = auction.EndTime;
        await auctionNotificationService.NotifyNewBidAsync(bid.AuctionId,bidDto);
        return bidDto;


    }
}

using FluentValidation;
using LiveAuction.Application.Interfaces;
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
        var bid = request.Adapt<Bid>();
        var addCurrentBidResult = await _auctionRepository.AddCurrentBidAsync(request.AuctionId, bid.Amount, cancellationToken);
        if (!addCurrentBidResult)
        {
            logger.LogWarning("Failed to add current bid for AuctionId: {AuctionId}",request.AuctionId);
            return new Error(ErrorCodes.ValidationError, "Failed to add current");
        }
        await bidRepository.AddAsync(bid, cancellationToken);
        logger.LogInformation("Bid created with Id: {BidId} for AuctionId: {AuctionId}", bid.Id, request.AuctionId);
        var bidDto = bid.Adapt<BidDto>();
        bidDto.Bidder = bidder.FullName;
        await auctionNotificationService.NotifyNewBidAsync(bid.AuctionId,bidDto);
        return bidDto;


    }
}

using FluentValidation;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Auctions.Commands.UpdateAuction;

internal class UpdateAuctionCommandHandler(
    IAuctionRepository _auctionRepository,
    ILogger<UpdateAuctionCommandHandler> _logger,
    IValidator<UpdateAuctionCommand> _validator) : IRequestHandler<UpdateAuctionCommand, OneOf<Error, AuctionDto>>
{
    public async Task<OneOf<Error, AuctionDto>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UpdateAuctionCommand for ID: {Id}", request.Id);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            _logger.LogWarning("Validation failed for UpdateAuctionCommand: {Error}", firstError.ErrorMessage);
            return new Error(ErrorCodes.ValidationError, firstError.ErrorMessage);
        }

        var auction = await _auctionRepository.GetByIdWithBidsAsync(request.Id, cancellationToken);
        
        if (auction == null)
        {
            _logger.LogWarning("Auction not found with ID: {Id}", request.Id);
            return new Error(ErrorCodes.NotFoundError, "المزاد غير موجود");
        }

        if (auction.Bids.Count > 0)
        {
            _logger.LogWarning("Cannot update auction with ID: {Id} - Auction has {BidCount} existing bids", 
                request.Id, auction.Bids.Count);
            return new Error(ErrorCodes.ForbiddenError, "لا يمكن تعديل مزاد يحتوي على مزايدات");
        }
        if(!string.Equals(auction.CreatedById, request.SellerId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Unauthorized update attempt on auction ID: {Id} by Seller ID: {SellerId}", 
                request.Id, request.SellerId);
            return new Error(ErrorCodes.UnauthorizedError, "غير مسموح لك بحذف هذا المزاد لأنه ليس ملكك! 👮‍♂️");
        }
        request.Adapt(auction);

        await _auctionRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Auction updated successfully: {Id}", auction.Id);

        return auction.Adapt<AuctionDto>();
    }
}
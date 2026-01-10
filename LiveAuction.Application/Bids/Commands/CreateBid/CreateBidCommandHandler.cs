using FluentValidation;
using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services.AuctionServices;
using LiveAuction.Application.Services.BackgroundJobServices;
using LiveAuction.Application.Services.BidServices;
using LiveAuction.Application.Services.WalletServices;
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

internal class CreateBidCommandHandler(
    ILogger<CreateBidCommandHandler> _logger,
    IValidator<CreateBidCommand> _validator,
    IBidService _bidService
    ) : IRequestHandler<CreateBidCommand, OneOf<Error, BidDto>>
{
    public async Task<OneOf<Error, BidDto>> Handle(CreateBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateBidCommand for AuctionId: {AuctionId}, UserId: {UserId}, Amount: {Amount}",
            request.AuctionId, request.UserId, request.Amount);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for CreateBidCommand: {Errors}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        
        return await _bidService.AddBidAsync(request, cancellationToken);
    }
}

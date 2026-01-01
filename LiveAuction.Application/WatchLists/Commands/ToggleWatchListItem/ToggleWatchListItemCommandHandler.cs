using FluentValidation;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.WatchLists.Commands.ToggleWatchListItem;

internal class ToggleWatchListItemCommandHandler(
    IWatchListRepository _watchListRepository,
    IAuctionRepository _auctionRepository,
    ILogger<ToggleWatchListItemCommandHandler> _logger,
    IValidator<ToggleWatchListItemCommand> _validator) : IRequestHandler<ToggleWatchListItemCommand, OneOf<Error, ToggleWatchListItemResponse>>
{
    public async Task<OneOf<Error, ToggleWatchListItemResponse>> Handle(ToggleWatchListItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling AddToWatchListCommand for UserId: {UserId}, AuctionId: {AuctionId}", request.UserId, request.AuctionId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for AddToWatchListCommand: {ErrorMessage}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
        if (auction == null)
        {
            _logger.LogWarning("Auction with ID {AuctionId} not found", request.AuctionId);
            return new Error(ErrorCodes.NotFoundError, $"Auction with ID {request.AuctionId} not found.");
        }

        var toggleResult = await _watchListRepository
            .ToggleAndReturnCurrentStateOfExitanceAsync(request.UserId,auction, cancellationToken);
        _logger.LogInformation("Toggled watch list item for UserId: {UserId}, AuctionId: {AuctionId}. Now in watch list: {IsInWatchList}", request.UserId, request.AuctionId, toggleResult);
        var response = new ToggleWatchListItemResponse { IsInWatchList = toggleResult };
        return response;

    }
}

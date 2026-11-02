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
    ILogger<ToggleWatchListItemCommandHandler> _logger) : IRequestHandler<ToggleWatchListItemCommand, OneOf<Error, ToggleWatchListItemResponse>>
{
    public async Task<OneOf<Error, ToggleWatchListItemResponse>> Handle(ToggleWatchListItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling AddToWatchListCommand for UserId: {UserId}, AuctionId: {AuctionId}", request.UserId, request.AuctionId);
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

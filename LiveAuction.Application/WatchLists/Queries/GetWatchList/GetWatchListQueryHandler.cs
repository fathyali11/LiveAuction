using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.WatchLists.Queries.GetWatchList;

internal class GetWatchListQueryHandler(
    IWatchListRepository _watchListRepository,
    ILogger<GetWatchListQueryHandler> _logger) : IRequestHandler<GetWatchListQuery, OneOf<Error, WatchListDto>>
{
    public async Task<OneOf<Error, WatchListDto>> Handle(GetWatchListQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting watchlist for user {UserId}", request.UserId);
        var watchListDto = await _watchListRepository.GetWatchListItems(request.UserId, cancellationToken);

        _logger.LogInformation("Found {Count} items in watchlist for user {UserId}", watchListDto.Items.Count, request.UserId);
        return watchListDto;
    }
}
using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.WatchLists.Queries.GetWatchList;

internal class GetWatchListQueryHandler(
    IWatchlistService _watchlistService,
    ILogger<GetWatchListQueryHandler> _logger) : IRequestHandler<GetWatchListQuery, OneOf<Error, PaginatedResult<WatchListItemDto>>>
{
    public async Task<OneOf<Error, PaginatedResult<WatchListItemDto>>> Handle(GetWatchListQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting watchlist for user {UserId}", request.UserId);
        var result = await _watchlistService.GetWatchlistAsync(request.UserId, request.PaginatedRequest, cancellationToken);
        _logger.LogInformation("Successfully retrieved watchlist for user {UserId}", request.UserId);
        return result;
    }
}
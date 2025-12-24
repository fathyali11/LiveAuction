using LiveAuction.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Application.WatchLists.Queries.GetWatchListCount;

internal class GetWatchListCountQueryHandler(
    IWatchListRepository _watchListRepository,
    ILogger<GetWatchListCountQueryHandler> _logger) : IRequestHandler<GetWatchListCountQuery, int>
{
    public async Task<int> Handle(GetWatchListCountQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting watchlist count for user {UserId}", request.UserId);

        try
        {
            var count = await _watchListRepository.GetCountAsync(request.UserId, cancellationToken);
            _logger.LogInformation("Watchlist count for user {UserId}: {Count}", request.UserId, count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting watchlist count for user {UserId}", request.UserId);
            return 0;
        }
    }
}
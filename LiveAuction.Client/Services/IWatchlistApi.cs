using LiveAuction.Shared.DTOs;
using Refit;

namespace LiveAuction.Client.Services;

public interface IWatchlistApi 
{
    [Get("/api/watchlists")]
    Task<WatchListDto> GetWatchlistAsync(CancellationToken cancellationToken = default);
    
    [Get("/api/watchlists/count")]
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    
    [Post("/api/watchlists/toggle")]
    Task<HttpResponseMessage> ToggleAsync([Body] ToggleWatchListItemRequest request, CancellationToken cancellationToken = default);
    
}
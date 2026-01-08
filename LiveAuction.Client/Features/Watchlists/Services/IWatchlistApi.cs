using LiveAuction.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Refit;

namespace LiveAuction.Client.Features.Watchlists.Services;

public interface IWatchlistApi 
{
    [Get("/api/watchlists")]
    Task<HttpResponseMessage> GetWatchlistAsync([Query] PaginatedRequest paginatedRequest, CancellationToken cancellationToken = default);
    
    [Get("/api/watchlists/count")]
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    
    [Post("/api/watchlists/toggle/{id}")]
    Task<HttpResponseMessage> ToggleAsync(int id, CancellationToken cancellationToken = default);
    
}
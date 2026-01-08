using LiveAuction.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Services.WatchlistServices;

internal interface IWatchlistService
{
    Task<PaginatedResult<WatchListItemDto>> GetWatchlistAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken);
    Task<bool> ClearWatchlistAsync(string userId, CancellationToken cancellationToken);
}

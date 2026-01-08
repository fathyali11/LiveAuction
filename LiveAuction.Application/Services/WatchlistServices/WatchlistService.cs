using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Services.WatchlistServices;

internal class WatchlistService(IWatchListRepository _watchListRepository): IWatchlistService
{
    public async Task<PaginatedResult<WatchListItemDto>> GetWatchlistAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        paginatedRequest.PageNumber = paginatedRequest.PageNumber <= 0 ? 1 : paginatedRequest.PageNumber;
        paginatedRequest.PageNumber = paginatedRequest.PageNumber <= 0 ? 8 : paginatedRequest.PageNumber;
        var (watchlistItems, totalCount) = await _watchListRepository.GetWatchListItemAndItsCountAsync(userId, paginatedRequest, cancellationToken);
        return new PaginatedResult<WatchListItemDto>
        {
            Items = watchlistItems,
            TotalCount = totalCount,
            PageNumber = paginatedRequest.PageNumber,
            PageSize = paginatedRequest.PageSize
        }; 
    }
    public async Task<bool> ClearWatchlistAsync(string userId, CancellationToken cancellationToken)
    {
        return await _watchListRepository.ClearAsync(userId, cancellationToken);
    }
}

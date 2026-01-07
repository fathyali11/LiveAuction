using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Services.WatchlistServices;

internal class WatchlistService(IWatchListRepository _watchListRepository): IWatchlistService
{
    public async Task<PaginatedResult<WatchListItemDto>> GetWatchlistAsync(string userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var (watchlistItems, totalCount) = await _watchListRepository.GetWatchListItemAndItsCountAsync(userId, pageNumber,pageSize, cancellationToken);
        return new PaginatedResult<WatchListItemDto>
        {
            Items = watchlistItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        }; 
    }
}

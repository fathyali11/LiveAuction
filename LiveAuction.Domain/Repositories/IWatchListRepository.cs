using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Domain.Repositories;

public interface IWatchListRepository
{
    Task<bool> ToggleAndReturnCurrentStateOfExitanceAsync(string userId, Auction auction, CancellationToken cancellationToken = default);
    Task<bool> ClearAsync(string userId, CancellationToken cancellationToken = default);
    Task<(List<WatchListItemDto> items, int count)> GetWatchListItemAndItsCountAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(string ?userId, CancellationToken cancellationToken = default);
}
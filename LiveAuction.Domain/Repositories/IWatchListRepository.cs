using LiveAuction.Shared.DTOs;

namespace LiveAuction.Domain.Repositories;

public interface IWatchListRepository
{
    Task<bool> ToggleAndReturnCurrentStateOfExitanceAsync(string userId, ToggleWatchListItemRequest request, CancellationToken cancellationToken = default);
    Task ClearAsync(string userId, CancellationToken cancellationToken = default);
    Task<WatchListDto> GetWatchListItems(string userId, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(string ?userId, CancellationToken cancellationToken = default);
}
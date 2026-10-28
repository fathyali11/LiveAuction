using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Domain.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds notifications to the change tracker without saving. Used when notifications must be
    /// committed atomically with another unit of work (for example, a bid).
    /// </summary>
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetCountUnRead(string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsRead(string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(string userId, int notificationId, CancellationToken cancellationToken = default);

}

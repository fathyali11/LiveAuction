using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Domain.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetCountUnRead(string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsRead(string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(string userId, int notificationId, CancellationToken cancellationToken = default);

}

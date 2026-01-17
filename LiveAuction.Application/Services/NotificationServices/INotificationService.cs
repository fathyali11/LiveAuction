using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.Services.NotificationServices;

internal interface INotificationService
{
    Task AddNotificationAsync(NotificationDto createNotificationDto, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetCountUnRead(string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsRead(string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(string userId, int notificationId, CancellationToken cancellationToken = default);

}

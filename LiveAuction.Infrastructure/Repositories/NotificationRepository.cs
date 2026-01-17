using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Repositories;

internal class NotificationRepository(ApplicationDbContext _context) : INotificationRepository
{

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Take(20)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto
            {
                Id=x.Id,
                UserId = x.UserId,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                NotificationType = x.NotificationType.ToString(),
                CreatedAt = x.CreatedAt,
                RelatedEntityId = x.RelatedEntityId
            }).ToListAsync(cancellationToken);
        return notifications;
    }
    public async Task<int> GetCountUnRead(string userId, CancellationToken cancellationToken = default)
    {
        var count = await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Take(20)
            .CountAsync(x => !x.IsRead, cancellationToken);
        return count;
    }
    public async Task<bool> MarkAllAsRead(string userId, CancellationToken cancellationToken = default)
    {
        var isUpdated = await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(x => x.SetProperty(n => n.IsRead, true), cancellationToken);
        return isUpdated > 0;
    }
    public async Task<bool> MarkAsReadAsync(string userId, int notificationId, CancellationToken cancellationToken = default)
    {
        var isMarkedAsRead = await _context.Notifications
            .Where(x => x.UserId == userId && x.Id == notificationId && !x.IsRead)
            .ExecuteUpdateAsync(x => x.SetProperty(n => n.IsRead, true), cancellationToken);
        return isMarkedAsRead > 0;
    }
}

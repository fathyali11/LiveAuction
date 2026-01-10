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
            .Select(x => new NotificationDto
            (
                x.UserId,
                x.Title,
                x.Message,
                x.IsRead,
                x.NotificationType,
                x.RelatedEntityId.Value
                )).ToListAsync(cancellationToken);
        return notifications;
    }
    public async Task<int> GetCountUnRead(string userId, CancellationToken cancellationToken = default)
    {
        var count = await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsRead)
            .CountAsync(cancellationToken);
        return count;
    }
    public async Task<bool> MarkAllAsRead(string userId, CancellationToken cancellationToken = default)
    {
        var isUpdated = await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(x => x.SetProperty(n => n.IsRead, true), cancellationToken);
        return isUpdated > 0;
    }
}

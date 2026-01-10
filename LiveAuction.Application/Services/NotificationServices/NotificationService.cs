using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Services.NotificationServices;

internal class NotificationService(IServiceProvider _serviceProvider,
    INotificationRepository notificationRepository) : INotificationService
{
    public async Task AddNotificationAsync(NotificationDto createNotificationDto, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var _notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var notification = new Notification
        {
            Title = createNotificationDto.Title,
            Message = createNotificationDto.Message,
            UserId = createNotificationDto.UserId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            NotificationType = createNotificationDto.NotificationType,
            RelatedEntityId = createNotificationDto.RelatedEntityId
        };
        await _notificationRepository.AddAsync(notification, cancellationToken);
    }
    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var notifications = await notificationRepository.GetUserNotificationsAsync(userId, cancellationToken);
        return notifications;
    }
    public async Task<int> GetCountUnRead(string userId, CancellationToken cancellationToken = default)
    {
        var count = await notificationRepository.GetCountUnRead(userId, cancellationToken);
        return count;
    }
    public async Task<bool> MarkAllAsRead(string userId, CancellationToken cancellationToken = default)
    {
        var isUpdated = await notificationRepository.MarkAllAsRead(userId, cancellationToken);
        return isUpdated;
    }
}

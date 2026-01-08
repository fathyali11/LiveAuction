using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Services.NotificationServices;

internal class NotificationService(IServiceProvider _serviceProvider) : INotificationService
{
    public async Task AddNotificationAsync(CreateNotificationDto createNotificationDto, CancellationToken cancellationToken = default)
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
}

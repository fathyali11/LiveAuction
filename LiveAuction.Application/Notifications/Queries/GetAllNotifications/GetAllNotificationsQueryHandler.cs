using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Notifications.Queries.GetAllNotifications;

internal class GetAllNotificationsQueryHandler(
    INotificationService _notificationService,
    ILogger<GetAllNotificationsQueryHandler> _logger
    ) : IRequestHandler<GetAllNotificationsQuery, OneOf<Error, List<NotificationDto>>>
{
    public async Task<OneOf<Error, List<NotificationDto>>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAllNotificationsQuery for UserId: {UserId}", request.UserId);
        var notifications = await _notificationService.GetUserNotificationsAsync(request.UserId, cancellationToken);
        _logger.LogInformation("Retrieved {Count} notifications for UserId: {UserId}", notifications.Count, request.UserId);
        return notifications;
    }
}

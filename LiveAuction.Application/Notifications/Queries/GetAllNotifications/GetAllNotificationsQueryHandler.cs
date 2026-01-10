using FluentValidation;
using LiveAuction.Application.Services.NotificationServices;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Notifications.Queries.GetAllNotifications;

internal class GetAllNotificationsQueryHandler(
    INotificationService _notificationService,
    ILogger<GetAllNotificationsQueryHandler> _logger,
    IValidator<GetAllNotificationsQuery> _validator
    ) : IRequestHandler<GetAllNotificationsQuery, OneOf<Error, List<NotificationDto>>>
{
    public async Task<OneOf<Error, List<NotificationDto>>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAllNotificationsQuery for UserId: {UserId}", request.UserId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for GetAllNotificationsQuery: {ErrorMessage}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var notifications = await _notificationService.GetUserNotificationsAsync(request.UserId, cancellationToken);
        _logger.LogInformation("Retrieved {Count} notifications for UserId: {UserId}", notifications.Count, request.UserId);
        return notifications;
    }
}

using FluentValidation;
using LiveAuction.Application.Services.NotificationServices;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Notifications.Commands.MarkAsRead;

internal class MarkAsReadCommandHandler(
    INotificationService _notificationService,
    ILogger<MarkAsReadCommandHandler> _logger,
    IValidator<MarkAsReadCommand> _validator
    ) : IRequestHandler<MarkAsReadCommand, OneOf<Error, bool>>
{
    public async Task<OneOf<Error, bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling MarkAsReadCommand for UserId: {UserId}, NotificationId: {NotificationId}", request.UserId, request.NotificationId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for MarkAsReadCommand: {ErrorMessage}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        _logger.LogInformation("Validation succeeded for MarkAsReadCommand");
        var isMarkedAsRead = await _notificationService.MarkAsReadAsync(request.UserId, request.NotificationId, cancellationToken);
        if(!isMarkedAsRead)
        {
            _logger.LogWarning("Failed to mark notification as read for UserId: {UserId}, NotificationId: {NotificationId}", request.UserId, request.NotificationId);
            return new Error(ErrorCodes.NotFoundError, "Notification not found or could not be marked as read.");
        }
        _logger.LogInformation("Successfully marked notification as read for UserId: {UserId}, NotificationId: {NotificationId}", request.UserId, request.NotificationId);
        return isMarkedAsRead;
    }
}

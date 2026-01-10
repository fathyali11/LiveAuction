using FluentValidation;
using LiveAuction.Application.Services.NotificationServices;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Notifications.Commands.MarkAllAsRead;

internal class MarkAllAsReadCommandHandler(
    INotificationService _notificationService,
    ILogger<MarkAllAsReadCommandHandler> _logger,
    IValidator<MarkAllAsReadCommand> _validator
    ) : IRequestHandler<MarkAllAsReadCommand, OneOf<Error, bool>>
{
    public async Task<OneOf<Error, bool>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling MarkAllAsReadCommand for UserId: {UserId}", request.UserId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for MarkAllAsReadCommand: {ErrorMessage}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var isUpdated = await _notificationService.MarkAllAsRead(request.UserId, cancellationToken);
        if(!isUpdated)
        {
            _logger.LogWarning("No notifications were marked as read for UserId: {UserId}", request.UserId);
            return new Error(ErrorCodes.NotFoundError, "No unread notifications found to mark as read.");
        }
        _logger.LogInformation("Successfully marked all notifications as read for UserId: {UserId}", request.UserId);
        return isUpdated;
    }
}

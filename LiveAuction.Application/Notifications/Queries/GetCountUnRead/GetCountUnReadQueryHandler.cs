using FluentValidation;
using LiveAuction.Application.Services.NotificationServices;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Notifications.Queries.GetCountUnRead;

internal class GetCountUnReadQueryHandler(
    INotificationService _notificationService,
    ILogger<GetCountUnReadQueryHandler> _logger,
    IValidator<GetCountUnReadQuery> _validator
    ) : IRequestHandler<GetCountUnReadQuery, OneOf<Error, int>>
{
    public async Task<OneOf<Error, int>> Handle(GetCountUnReadQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetCountUnReadQuery for UserId: {UserId}", request.UserId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for GetCountUnReadQuery: {ErrorMessage}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var count = await _notificationService.GetCountUnRead(request.UserId, cancellationToken);
        _logger.LogInformation("Retrieved {Count} unread notifications for UserId: {UserId}", count, request.UserId);
        return count;
    }
}

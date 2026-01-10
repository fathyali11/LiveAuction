using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Notifications.Commands.MarkAsRead;

internal class MarkAsReadCommandValidator:AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.NotificationId).GreaterThan(0).WithMessage("NotificationId must be greater than zero.");
    }
}

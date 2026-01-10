using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Notifications.Commands.MarkAllAsRead;

internal class MarkAllAsReadCommandValidator:AbstractValidator<MarkAllAsReadCommand>
{
    public MarkAllAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .NotNull().WithMessage("UserId cannot be null.");
    }
}

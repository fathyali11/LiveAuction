using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Notifications.Queries.GetCountUnRead;

internal class GetCountUnReadQueryValidator:AbstractValidator<GetCountUnReadQuery>
{
    public GetCountUnReadQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .NotNull().WithMessage("UserId cannot be null.");
    }
}

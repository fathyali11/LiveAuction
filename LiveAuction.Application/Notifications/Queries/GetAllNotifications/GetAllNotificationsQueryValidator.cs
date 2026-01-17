using FluentValidation;

namespace LiveAuction.Application.Notifications.Queries.GetAllNotifications;

internal class GetAllNotificationsQueryValidator:AbstractValidator<GetAllNotificationsQuery>
{
    public GetAllNotificationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .NotNull().WithMessage("UserId cannot be null.");
    }
}

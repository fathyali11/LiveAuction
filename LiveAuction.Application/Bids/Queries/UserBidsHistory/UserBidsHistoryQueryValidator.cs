using FluentValidation;

namespace LiveAuction.Application.Bids.Queries.UserBidsHistory;

internal class UserBidsHistoryQueryValidator:AbstractValidator<UserBidsHistoryQuery>
{
    public UserBidsHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.");
    }
}

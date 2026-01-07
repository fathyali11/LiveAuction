using FluentValidation;

namespace LiveAuction.Application.WatchLists.Queries.GetWatchList;

internal class GetWatchListQueryValidator : AbstractValidator<GetWatchListQuery>
{
    public GetWatchListQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .MaximumLength(100).WithMessage("UserId cannot exceed 100 characters.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
    }
}
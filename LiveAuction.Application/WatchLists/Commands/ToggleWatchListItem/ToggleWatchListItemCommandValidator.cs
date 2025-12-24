using FluentValidation;

namespace LiveAuction.Application.WatchLists.Commands.ToggleWatchListItem;

internal class ToggleWatchListItemCommandValidator:AbstractValidator<ToggleWatchListItemCommand>
{
    public ToggleWatchListItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.AuctionId)
            .GreaterThan(0).WithMessage("AuctionId must be greater than zero.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");
        RuleFor(x => x.ImageName)
            .NotEmpty().WithMessage("ImageName is required.")
            .MaximumLength(100).WithMessage("ImageName cannot exceed 100 characters.");
        RuleFor(x => x.CurrentPrice)
            .GreaterThanOrEqualTo(0).WithMessage("CurrentPrice must be non-negative.");
    }
}

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

    }
}

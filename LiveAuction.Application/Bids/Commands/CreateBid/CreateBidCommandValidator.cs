using FluentValidation;
using LiveAuction.Domain.Consts;

namespace LiveAuction.Application.Bids.Commands.CreateBid;

internal class CreateBidCommandValidator : AbstractValidator<CreateBidCommand>
{
    public CreateBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .GreaterThan(0).WithMessage("AuctionId must be greater than 0.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("An idempotency key is required.")
            .MaximumLength(BidRules.IdempotencyKeyMaxLength)
            .WithMessage($"The idempotency key cannot exceed {BidRules.IdempotencyKeyMaxLength} characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than 0.")
            .LessThanOrEqualTo(BidRules.MaximumAmount)
            .WithMessage($"Bid amount cannot exceed {BidRules.MaximumAmount:N0}.")
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("Bid amount cannot have more than two decimal places.");
    }
}

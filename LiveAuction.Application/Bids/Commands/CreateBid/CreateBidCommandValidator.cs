using FluentValidation;

namespace LiveAuction.Application.Bids.Commands.CreateBid;

internal class CreateBidCommandValidator:AbstractValidator<CreateBidCommand>
{
    public CreateBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .GreaterThan(0).WithMessage("AuctionId must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than 0.");
    }
}

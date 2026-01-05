using FluentValidation;

namespace LiveAuction.Application.Wallets.Queries.GetWalletSummary;

internal class GetWalletSummaryQueryValidator:AbstractValidator<GetWalletSummaryQuery>
{
    public GetWalletSummaryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

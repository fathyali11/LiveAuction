using FluentValidation;

namespace LiveAuction.Application.Wallets.Queries.GetTransactions;

internal class GetTransactionsQueryValidator:AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.");
    }

}

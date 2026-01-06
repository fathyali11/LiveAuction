using FluentValidation;

namespace LiveAuction.Application.Auctions.Queries.GetAllAuctions;

internal class GetAllAuctionsQueryValidator : AbstractValidator<GetAllAuctionsQuery>
{
    public GetAllAuctionsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.");
    }
}
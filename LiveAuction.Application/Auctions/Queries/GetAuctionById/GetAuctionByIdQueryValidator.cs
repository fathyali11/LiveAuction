using FluentValidation;

namespace LiveAuction.Application.Auctions.Queries.GetAuctionById;

internal class GetAuctionByIdQueryValidator:AbstractValidator<GetAuctionByIdQuery>
{
    public GetAuctionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف المزاد مطلوب")
            .GreaterThan(0).WithMessage("معرف المزاد غير صالح");
    }
}

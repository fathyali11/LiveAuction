using FluentValidation;

namespace LiveAuction.Application.Auctions.Commands.DeleteAuction;

internal class DeleteAuctionCommandValidator:AbstractValidator<DeleteAuctionCommand>
{
    public DeleteAuctionCommandValidator()
    {
        RuleFor(x=>x.Id)
            .NotEmpty().WithMessage("معرف المزاد مطلوب")
            .GreaterThan(0).WithMessage("معرف المزاد غير صالح");
    }
}

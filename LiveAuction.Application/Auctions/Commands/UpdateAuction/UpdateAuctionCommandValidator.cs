using FluentValidation;

namespace LiveAuction.Application.Auctions.Commands.UpdateAuction;

internal class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    public UpdateAuctionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("„⁄—› «·„“«œ €Ì— ’«·Õ");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("⁄‰Ê«‰ «·„“«œ „ÿ·Ê»")
            .MaximumLength(200).WithMessage("⁄‰Ê«‰ «·„“«œ ÌÃ» √‰ ÌﬂÊ‰ √ﬁ· „‰ 200 Õ—›");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ê’› «·„“«œ „ÿ·Ê»")
            .MaximumLength(1000).WithMessage("Ê’› «·„“«œ ÌÃ» √‰ ÌﬂÊ‰ √ﬁ· „‰ 1000 Õ—›");
    }
}
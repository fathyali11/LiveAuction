using FluentValidation;

namespace LiveAuction.Application.Auctions.Commands.CreateAuction;

internal class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("⁄‰Ê«‰ «·„“«œ „ÿ·Ê»")
            .MaximumLength(200).WithMessage("⁄‰Ê«‰ «·„“«œ ÌÃ» √‰ ÌﬂÊ‰ √ﬁ· „‰ 200 Õ—›");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ê’› «·„“«œ „ÿ·Ê»")
            .MaximumLength(1000).WithMessage("Ê’› «·„“«œ ÌÃ» √‰ ÌﬂÊ‰ √ﬁ· „‰ 1000 Õ—›");

        RuleFor(x => x.StartTime)
            .Must(startTime => startTime >= DateTime.UtcNow.AddMinutes(-2))
            .WithMessage("Êﬁ  »œ¡ «·„“«œ ÌÃ» √‰ ÌﬂÊ‰ «·¬‰ √Ê ›Ì «·„” ﬁ»·");

        RuleFor(x => x.StartingBid)
            .GreaterThan(0).WithMessage("”⁄— «·»œ«Ì… ÌÃ» √‰ ÌﬂÊ‰ √ﬂ»— „‰ ’›—");

        RuleFor(x => x.DurationInMinutes)
            .GreaterThan(0).WithMessage("„œ… «·„“«œ ÌÃ» √‰  ﬂÊ‰ √ﬂ»— „‰ ’›—")
            .LessThanOrEqualTo(10080).WithMessage("„œ… «·„“«œ ÌÃ» √‰  ﬂÊ‰ √ﬁ· „‰ √”»Ê⁄");

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("„⁄—› «·„” Œœ„ „ÿ·Ê»");

        RuleFor(x => x.CreatedByName)
            .NotEmpty().WithMessage("«”„ «·„” Œœ„ „ÿ·Ê»");
    }
}
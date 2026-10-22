using FluentValidation;
using LiveAuction.Application.Common.Validation;

namespace LiveAuction.Application.Auctions.Commands.CreateAuction;

internal class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان المزاد مطلوب")
            .MaximumLength(200).WithMessage("عنوان المزاد يجب أن يكون أقل من 200 حرف");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("وصف المزاد مطلوب")
            .MaximumLength(1000).WithMessage("وصف المزاد يجب أن يكون أقل من 1000 حرف");

        RuleFor(x => x.Image)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("صورة المزاد مطلوبة")
            .Must(ImageFileValidation.HasAllowedSize)
                .WithMessage($"حجم صورة المزاد يجب أن يكون أقل من {ImageFileValidation.MaxFileSizeInBytes / (1024 * 1024)} ميجابايت")
            .Must(ImageFileValidation.HasAllowedExtension)
                .WithMessage("امتداد صورة المزاد يجب أن يكون JPG أو JPEG أو PNG")
            .Must(ImageFileValidation.HasAllowedContentType)
                .WithMessage("نوع صورة المزاد يجب أن يكون JPEG أو PNG")
            .Must(ImageFileValidation.HasValidSignature)
                .WithMessage("محتوى صورة المزاد غير صالح");

        RuleFor(x => x.StartTime)
            .Must(startTime => startTime >= DateTime.UtcNow.AddMinutes(-2))
            .WithMessage("وقت بدء المزاد يجب أن يكون الآن أو في المستقبل");

        RuleFor(x => x.StartingBid)
            .GreaterThan(0).WithMessage("سعر البداية يجب أن يكون أكبر من صفر");

        RuleFor(x => x.DurationInMinutes)
            .GreaterThan(0).WithMessage("مدة المزاد يجب أن تكون أكبر من صفر")
            .LessThanOrEqualTo(10080).WithMessage("مدة المزاد يجب أن تكون أقل من أسبوع");

    }
}

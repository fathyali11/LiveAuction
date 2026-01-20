using FluentValidation;

namespace LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;

internal class RegisterUserCommandValidator:AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName)
              .NotEmpty().WithMessage("الاسم بالكامل مطلوب.")
              .MaximumLength(100).WithMessage("يجب ألا يتجاوز الاسم 100 حرف.");

        RuleFor(x => x.Email)
             .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
             .EmailAddress().WithMessage("يرجى إدخال بريد إلكتروني صالح.");

        RuleFor(x => x.Password)
             .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
             .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W)\S{8,}$")
             .WithMessage("يجب أن تتكون كلمة المرور من 8 أحرف على الأقل، وتحتوي على حرف كبير، وحرف صغير، ورقم، ورمز خاص.");

    }
}

using FluentValidation;
using LiveAuction.Application.ApplicationUsers.Commands.LoginUser;

namespace LiveAuction.Application.ApplicationUsers.Commands.LoginUser;

internal class LoginUserCommandValidator:AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
              .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
              .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.Password)
             .NotEmpty().WithMessage("كلمة المرور مطلوبة.");
    }
}

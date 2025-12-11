using FluentValidation;
using LiveAuction.Application.ApplicationUsers.Commands.LoginUser;

namespace LiveAuction.Application.ApplicationUsers.Commands.LoginUser;

internal class LoginUserCommandValidator:AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

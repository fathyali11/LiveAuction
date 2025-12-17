using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.ApplicationUsers.Commands.RefreshToken;

internal class RefreshTokenCommandValidator: AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .Must(token => !string.IsNullOrWhiteSpace(token)).WithMessage("Token cannot be whitespace.");
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .Must(refreshToken => !string.IsNullOrWhiteSpace(refreshToken)).WithMessage("Refresh token cannot be whitespace.");
    }
}

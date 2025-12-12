using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Services;
using OneOf;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.ApplicationUsers.Commands.LoginUser;

internal class LoginUserCommandHandler(UserManager<ApplicationUser> _userManager,
    ILogger<LoginUserCommandHandler> _logger,
    IAuthService _authService,
    IValidator<LoginUserCommand> _validator) : IRequestHandler<LoginUserCommand, OneOf<Error, AuthResponse>>
{
    public async Task<OneOf<Error, AuthResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling LoginUserCommand for email: {Email}", request.Email);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            _logger.LogWarning("Validation failed for LoginUserCommand: {Error}", firstError.ErrorMessage);
            return new Error(ErrorCodes.ValidationError, firstError.ErrorMessage);
        }
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", request.Email);
            return new Error(ErrorCodes.NotFoundError, "User not found.");
        }
        var checkPasswordResult = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!checkPasswordResult)
        {
            _logger.LogWarning("Invalid password for user: {Email}", request.Email);
            return new Error(ErrorCodes.UnauthorizedError, "Invalid credentials.");
        }
        var roles = await _userManager.GetRolesAsync(user);
        var tokenCreationResult = await _authService.GenerateJwtTokenAsync(user,roles, cancellationToken);
        await _userManager.UpdateAsync(user);
        _logger.LogInformation("User logged in successfully: {Email}", request.Email);
        return new AuthResponse
        {
            Email = user.Email!,
            Token = tokenCreationResult.Item1,
            ExpiresAt = tokenCreationResult.Item2,
            Roles = roles.ToList()
        };
    }
}

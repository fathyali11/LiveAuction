using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using OneOf;
using LiveAuction.Shared.DTOs;
using LiveAuction.Application.Services;
using RefreshTokenEntity = LiveAuction.Domain.Entities.RefreshToken;

namespace LiveAuction.Application.ApplicationUsers.Commands.LoginUser;

internal class LoginUserCommandHandler(UserManager<ApplicationUser> _userManager,
    ILogger<LoginUserCommandHandler> _logger,
    IAuthService _authService,
    IRefreshTokenService _refreshTokenService) : IRequestHandler<LoginUserCommand, OneOf<Error, AuthResponse>>
{
     private const string InvalidCredentialsMessage = "بيانات الدخول غير صحيحة.";

    public async Task<OneOf<Error, AuthResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling LoginUserCommand for email: {Email}", request.Email);
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: no user found for email: {Email}", request.Email);
            return new Error(ErrorCodes.UnauthorizedError, InvalidCredentialsMessage);
        }
        if (await _userManager.IsLockedOutAsync(user))
        {
             _logger.LogWarning("Login failed: user {Email} is locked out until {LockoutEnd}", request.Email, user.LockoutEnd);
            return new Error(ErrorCodes.UnauthorizedError, InvalidCredentialsMessage);
        }
        var checkPasswordResult = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!checkPasswordResult)
        {
            await _userManager.AccessFailedAsync(user);
            _logger.LogWarning("Login failed: invalid password for user: {Email}", request.Email);
            return new Error(ErrorCodes.UnauthorizedError, InvalidCredentialsMessage);
        }
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: user {Email} is deactivated.", request.Email);
            return new Error(ErrorCodes.UnauthorizedError, InvalidCredentialsMessage);
        }
        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var tokenDto = await _authService.GenerateJwtTokenAsync(user, roles, cancellationToken);

        await _refreshTokenService.RemoveInactiveAsync(user.Id, cancellationToken);
        await _refreshTokenService.AddAsync(new RefreshTokenEntity
        {
            Token = tokenDto.RefreshToken,
            ExpiresAt = tokenDto.RefreshTokenExpiration,
            ApplicationUserId = user.Id
        }, cancellationToken);

        _logger.LogInformation("User {Email} logged in successfully.", request.Email);
        return new AuthResponse
        {
            Token = tokenDto.Token,
            ExpiresAt = tokenDto.Expiration,
            RefreshToken = tokenDto.RefreshToken,
            RefreshTokenExpiresAt = tokenDto.RefreshTokenExpiration,
            Email = user.Email!,
            Roles = [.. roles]
        };
    }
}

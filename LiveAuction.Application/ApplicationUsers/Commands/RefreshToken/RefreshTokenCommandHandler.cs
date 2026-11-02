using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OneOf;
using RefreshTokenEntity = LiveAuction.Domain.Entities.RefreshToken;

namespace LiveAuction.Application.ApplicationUsers.Commands.RefreshToken;

internal class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> _userManager,
    ILogger<RefreshTokenCommandHandler> _logger,
    IAuthService _authService,
    IRefreshTokenService _refreshTokenService) : IRequestHandler<RefreshTokenCommand, OneOf<Error, AuthResponse>>
{
    private const string InvalidRefreshTokenMessage = "رمز التحديث غير صالح.";

    public async Task<OneOf<Error, AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling RefreshTokenCommand.");
        var userId = await _authService.GetUserIdFrom(request.Token, cancellationToken);
        if(userId is null)
        {
            _logger.LogWarning("Refresh failed: the access token is invalid.");
            return new Error(ErrorCodes.UnauthorizedError, InvalidRefreshTokenMessage);
        }
        var storedRefreshToken = await _refreshTokenService.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (storedRefreshToken is null)
        {
            _logger.LogWarning("Refresh failed: unknown refresh token for user {UserId}.", userId);
            return new Error(ErrorCodes.UnauthorizedError, InvalidRefreshTokenMessage);
        }
        if (!storedRefreshToken.IsActive)
        {
            _logger.LogWarning("Refresh failed: refresh token is expired or revoked for user {UserId}.", userId);
            return new Error(ErrorCodes.UnauthorizedError, InvalidRefreshTokenMessage);
        }
        
        if (storedRefreshToken.ApplicationUserId != userId)
        {
            _logger.LogWarning("Refresh failed: refresh token does not belong to user {UserId}.", userId);
            return new Error(ErrorCodes.UnauthorizedError, InvalidRefreshTokenMessage);
        }
        var user = await _userManager.FindByIdAsync(userId);
        if(user is null)
        {
            _logger.LogWarning("Refresh failed: user with id {UserId} was not found.", userId);
            return new Error(ErrorCodes.NotFoundError, InvalidRefreshTokenMessage);
        }
        if (!user.IsActive)
        {
            _logger.LogWarning("Refresh failed: user {UserId} is deactivated.", userId);
            return new Error(ErrorCodes.UnauthorizedError, InvalidRefreshTokenMessage);
        }
        var roles = await _userManager.GetRolesAsync(user);
        var tokenDto = await _authService.GenerateJwtTokenAsync(user, roles, cancellationToken);

        await _refreshTokenService.RemoveInactiveAsync(user.Id, cancellationToken);

        await _refreshTokenService.RotateAsync(storedRefreshToken, new RefreshTokenEntity
        {
            Token = tokenDto.RefreshToken,
            ExpiresAt = tokenDto.RefreshTokenExpiration,
            ApplicationUserId = user.Id
        }, cancellationToken);

        _logger.LogInformation("Refresh token rotated for user id: {UserId}", userId);
        return new AuthResponse
        {
            Token = tokenDto.Token,
            ExpiresAt = tokenDto.Expiration,
            RefreshToken = tokenDto.RefreshToken,
            RefreshTokenExpiresAt = tokenDto.RefreshTokenExpiration,
            Email = user.Email!,
            Roles = roles.ToList()
        };
    }
}

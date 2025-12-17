using FluentValidation;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Domain.Services;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.ApplicationUsers.Commands.RefreshToken;

internal class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> _userManager,
    IValidator<RefreshTokenCommand> _validator,
    ILogger<RefreshTokenCommandHandler> _logger,
    IAuthService _authService,
    IRefreshTokenRepository _refreshTokenRepository) : IRequestHandler<RefreshTokenCommand, OneOf<Error, AuthResponse>>
{
    public async Task<OneOf<Error, AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling refresh token command for token: {Token} and refresh token: {RefreshToken}", request.Token,request.RefreshToken);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            _logger.LogWarning("Validation failed for RefreshTokenCommand: {Error}", firstError.ErrorMessage);
            return new Error(ErrorCodes.ValidationError, firstError.ErrorMessage);
        }
        var userId = await _authService.GetUserIdFrom(request.Token, cancellationToken);
        if(userId is null)
        {
            var errorMessage = $"invalid token : {request.Token}";
            _logger.LogWarning(errorMessage);
            return new Error(ErrorCodes.UnauthorizedError, errorMessage);
        }
        var refreshToken = await _refreshTokenRepository.IsActiveAsync(request.RefreshToken, cancellationToken);
        if (refreshToken is null)
        {
            var errorMessage = $"invalid refresh token : {request.RefreshToken}";
            _logger.LogWarning(errorMessage);
            return new Error(ErrorCodes.UnauthorizedError, errorMessage);
        }
        if(!refreshToken.IsActive)
        {
            var errorMessage = $"refresh token is not active and there is a problem in refresh token : {request.RefreshToken}";
            _logger.LogWarning(errorMessage);
            return new Error(ErrorCodes.UnauthorizedError, errorMessage);
        }
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

        _logger.LogInformation("Refresh Token Revoked : {RefreshToken}", request.RefreshToken);
        var user = await _userManager.FindByIdAsync(userId);
        if(user is null)
        {
            var errorMessage = $"user with id : {userId} is not found";
            _logger.LogWarning(errorMessage);
            return new Error(ErrorCodes.NotFoundError, errorMessage);
        }
        var roles = await _userManager.GetRolesAsync(user);
        var tokenDto = await _authService.GenerateJwtTokenAsync(user, roles, cancellationToken);
        _logger.LogInformation("New Token Generated for user id: {UserId}", userId);
        user.RefreshTokens.Add(new LiveAuction.Domain.Entities.RefreshToken
        {
            Token = tokenDto.RefreshToken,
            ExpiresAt = tokenDto.RefreshTokenExpiration
        });
        await _userManager.UpdateAsync(user);
        _logger.LogInformation("New Refresh Token Stored for user id: {UserId}", userId);
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

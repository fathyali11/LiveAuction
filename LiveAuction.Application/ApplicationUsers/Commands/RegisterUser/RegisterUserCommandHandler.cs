using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Services;
using OneOf;
using System.Transactions;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;

internal class RegisterUserCommandHandler(UserManager<ApplicationUser>_userManager,
    ILogger<RegisterUserCommandHandler> _logger,
    IValidator<RegisterUserCommand> _validator,
    IAuthService _authService) : IRequestHandler<RegisterUserCommand, OneOf<Error, AuthResponse>>
{
    public async Task<OneOf<Error, AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling RegisterUserCommand for email: {Email}", request.Email);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            _logger.LogWarning("Validation failed for RegisterUserCommand: {Error}", firstError.ErrorMessage);
            return new Error(ErrorCodes.ValidationError, firstError.ErrorMessage);
        }
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User with email {Email} already exists", request.Email);
            return new Error(ErrorCodes.Duplicate, "User with this email already exists.");
        }
        var user=request.Adapt<ApplicationUser>();
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var firstError = createResult.Errors.First();
                _logger.LogError("Failed to create user: {Error}", firstError.Description);
                return new Error(ErrorCodes.InternalServerError, firstError.Description);
            }
            _logger.LogInformation("User with email {Email} created successfully", request.Email);
            var addToRoleResult = await _userManager.AddToRoleAsync(user, UserRoles.Customer);
            if (!addToRoleResult.Succeeded)
            {
                var firstError = addToRoleResult.Errors.First();
                _logger.LogError("Failed to assign role to user: {Error}", firstError.Description);
                return new Error(ErrorCodes.InternalServerError, firstError.Description);
            }
            transaction.Complete();
        }
        
        _logger.LogInformation("Role 'Customer' assigned to user with email {Email}", request.Email);
        var roles = await _userManager.GetRolesAsync(user);
        var tokenDto = await _authService.GenerateJwtTokenAsync(user, roles, cancellationToken);
        _logger.LogInformation("User {Email} logged in successfully.", request.Email);
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

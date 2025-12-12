using MediatR;
using LiveAuction.Domain.Consts;
using OneOf;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password
): IRequest<OneOf<Error, AuthResponse>>;

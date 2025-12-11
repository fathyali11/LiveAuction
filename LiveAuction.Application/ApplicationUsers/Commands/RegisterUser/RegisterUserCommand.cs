using MediatR;
using LiveAuction.Application.ApplicationUsers.Dtos;
using LiveAuction.Domain.Consts;
using OneOf;

namespace LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password
): IRequest<OneOf<Error, AuthResponse>>;

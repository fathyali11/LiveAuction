using LiveAuction.Domain.Consts;
using MediatR;
using OneOf;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.ApplicationUsers.Commands.LoginUser;
public record LoginUserCommand(string Email, string Password): IRequest<OneOf<Error,AuthResponse>>;

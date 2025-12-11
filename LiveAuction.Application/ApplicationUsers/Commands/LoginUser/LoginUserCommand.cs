using LiveAuction.Domain.Consts;
using MediatR;
using LiveAuction.Application.ApplicationUsers.Dtos;
using OneOf;

namespace LiveAuction.Application.ApplicationUsers.Commands.LoginUser;
public record LoginUserCommand(string Email, string Password): IRequest<OneOf<Error,AuthResponse>>;

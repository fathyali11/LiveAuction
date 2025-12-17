using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.ApplicationUsers.Commands.RefreshToken;
public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<OneOf<Error, AuthResponse>>;

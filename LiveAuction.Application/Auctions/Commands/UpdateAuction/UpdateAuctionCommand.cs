using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Auctions.Commands.UpdateAuction;

public record UpdateAuctionCommand(
    int Id,
    string Title,
    string Description,
    string SellerId,
    string? ImageUrl
) : IRequest<OneOf<Error, AuctionDto>>;
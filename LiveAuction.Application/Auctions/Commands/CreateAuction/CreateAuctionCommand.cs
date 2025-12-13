using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Auctions.Commands.CreateAuction;

public record CreateAuctionCommand(
    string Title,
    string Description,
    string? ImageUrl,
    DateTime StartTime,
    decimal StartingBid,
    int DurationInMinutes,
    string CreatedById,
    string CreatedByName
) : IRequest<OneOf<Error, AuctionDto>>;
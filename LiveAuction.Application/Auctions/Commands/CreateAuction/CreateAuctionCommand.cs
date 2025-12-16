using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using OneOf;

namespace LiveAuction.Application.Auctions.Commands.CreateAuction;

public record CreateAuctionCommand(
    string Title,
    string Description,
    IFormFile Image,
    DateTime StartTime,
    decimal StartingBid,
    int DurationInMinutes,
    string CreatedById,
    string CreatedByName
) : IRequest<OneOf<Error, AuctionDto>>;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;
using System.Text.Json.Serialization;

namespace LiveAuction.Application.Bids.Commands.CreateBid;

public record CreateBidCommand(
    int AuctionId,
    decimal Amount,
    string? IdempotencyKey=null)
 : IRequest<OneOf<Error, BidDto>>;
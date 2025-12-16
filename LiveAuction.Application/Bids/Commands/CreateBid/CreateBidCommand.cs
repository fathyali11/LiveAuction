using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;
using System.Text.Json.Serialization;

namespace LiveAuction.Application.Bids.Commands.CreateBid;

public class CreateBidCommand : IRequest<OneOf<Error, BidDto>>
{
    public int AuctionId { get; set; }
    public decimal Amount { get; set; }

    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
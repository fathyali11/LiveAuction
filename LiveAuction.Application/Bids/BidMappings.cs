using LiveAuction.Application.Bids.Commands.CreateBid;
using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Mapster;

namespace LiveAuction.Application.Bids;

internal class BidMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Bid, BidDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.TimePlaced, src => src.BidTime)
            .Map(dest => dest.AuctionId, src => src.AuctionId)
            .Map(dest => dest.Bidder, src => src.Bidder.FullName);


        config.NewConfig<CreateBidCommand, Bid>()
            .Map(dest => dest.AuctionId, src => src.AuctionId)
            .Map(dest => dest.BidderId, src => src.UserId)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.BidTime, _ => DateTime.UtcNow);
    }
}

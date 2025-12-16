using LiveAuction.Application.Auctions.Commands.CreateAuction;
using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Mapster;

namespace LiveAuction.Application.Auctions;

public class AuctionMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateAuctionCommand, Auction>()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.StartTime, src => src.StartTime)
            .Map(dest => dest.EndTime, src => src.StartTime.AddMinutes(src.DurationInMinutes))
            .Map(dest => dest.StartingBid, src => src.StartingBid)
            .Map(dest => dest.CurrentBid, src => src.StartingBid)
            .Map(dest => dest.CreatedById, src => src.CreatedById)
            .Map(dest => dest.Status, _=> AuctionStatus.Open);


        config.NewConfig<Auction, AuctionDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.ImageName, src => src.ImageName)
            .Map(dest => dest.StartTime, src => src.StartTime)
            .Map(dest => dest.EndTime, src => src.EndTime)
            .Map(dest => dest.StartingBid, src => src.StartingBid)
            .Map(dest => dest.CurrentBid, src => src.CurrentBid)
            .Map(dest => dest.Seller, src => src.CreatedBy.FullName)
            .Map(dest => dest.CurrentBidder, src => src.CurrentBidder)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Bids, src => src.Bids);

        
    }
}

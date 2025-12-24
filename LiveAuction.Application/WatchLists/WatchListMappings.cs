using LiveAuction.Application.WatchLists.Commands.ToggleWatchListItem;
using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Mapster;

namespace LiveAuction.Application.WatchLists;

internal class WatchListMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ToggleWatchListItemCommand, WatchListItem>()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.AuctionId, src => src.AuctionId)
            .Map(dest => dest.CurrentBid, src => src.CurrentPrice)
            .Map(dest => dest.EndTime, src => src.EndTime)
            .Map(dest => dest.ImageName, src => src.ImageName);

        config.NewConfig<ToggleWatchListItemCommand, ToggleWatchListItemRequest>()
            .Map(dest => dest.AuctionId, src => src.AuctionId)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.CurrentPrice, src => src.CurrentPrice)
            .Map(dest => dest.EndTime, src => src.EndTime)
            .Map(dest => dest.ImageName, src => src.ImageName);

        config.NewConfig<ToggleWatchListItemRequest, ToggleWatchListItemResponse>();
    }
}

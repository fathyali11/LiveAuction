using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.Interfaces;

public interface IAuctionNotificationService
{
    Task NotifyNewBidAsync(int auctionId, BidDto bidDto);
}

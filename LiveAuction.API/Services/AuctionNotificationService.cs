using LiveAuction.API.Hubs;
using LiveAuction.Application.Interfaces;
using LiveAuction.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace LiveAuction.API.Services;

public class AuctionNotificationService(IHubContext<AuctionHub> _hubContext) : IAuctionNotificationService
{
    public Task ForceRefreshWalletAsync(string userId)
    {
        return _hubContext.Clients.User(userId)
            .SendAsync("ForceRefreshWallet");

    }

    public async Task NotifyNewBidAsync(int auctionId,BidDto bidDto)
    {
        await _hubContext.Clients.Group(auctionId.ToString())
            .SendAsync("NewBidPlaced", bidDto);
    }
}

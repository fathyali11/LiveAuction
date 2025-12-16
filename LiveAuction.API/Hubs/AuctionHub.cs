using Microsoft.AspNetCore.SignalR;

namespace LiveAuction.API.Hubs;

public class AuctionHub(ILogger<AuctionHub> logger):Hub
{
    public async Task JoinToGroub(string auctionId)
    {
        logger.LogInformation("User joined to group {AuctionId}", auctionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);
    }
    public async Task LeaveFromGroub(string auctionId)
    {
        logger.LogInformation("User left from group {AuctionId}", auctionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
    }
}

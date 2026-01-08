using Microsoft.AspNetCore.SignalR;

namespace LiveAuction.API.Hubs;

public class AuctionHub(ILogger<AuctionHub> _logger):Hub
{
    public async Task JoinToGroup(string auctionId)
    {
        _logger.LogInformation("User joined to group {AuctionId}", auctionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);
    }
    public async Task LeaveFromGroup(string auctionId)
    {
        _logger.LogInformation("User left from group {AuctionId}", auctionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
    }
}

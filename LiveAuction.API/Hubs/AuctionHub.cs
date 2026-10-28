using LiveAuction.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LiveAuction.API.Hubs;

[Authorize]
public class AuctionHub(ILogger<AuctionHub> _logger) : Hub
{
    public async Task JoinToGroup(string auctionId)
    {
        if (!int.TryParse(auctionId, out var parsedAuctionId) || parsedAuctionId <= 0)
        {
            _logger.LogWarning("Rejected an invalid auction group id: {AuctionId}.", auctionId);
            return;
        }

        var groupName = parsedAuctionId.ToString();
        _logger.LogInformation("User joined auction group {AuctionId}.", parsedAuctionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveFromGroup(string auctionId)
    {
        if (!int.TryParse(auctionId, out var parsedAuctionId) || parsedAuctionId <= 0)
        {
            return;
        }

        var groupName = parsedAuctionId.ToString();
        _logger.LogInformation("User left auction group {AuctionId}.", parsedAuctionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}

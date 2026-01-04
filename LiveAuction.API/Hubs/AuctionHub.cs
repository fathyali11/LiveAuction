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
    public override async Task OnConnectedAsync()
    {
        var signalrConnectionId = Context.UserIdentifier;
        logger.LogInformation("User connected with SignalR ConnectionId: {ConnectionId}", signalrConnectionId);

        var user = Context.User;
        foreach (var claim in user?.Claims!)
        {
            logger.LogInformation("Claim Type: {ClaimType}, Claim Value: {ClaimValue}", claim.Type, claim.Value);
        }
        await base.OnConnectedAsync();
    }
}

namespace LiveAuction.Shared.DTOs;

public class UserBidDto
{
    public int AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public decimal MyHighestBid { get; set; }
    public decimal CurrentHighBid { get; set; }
    public DateTime AuctionEndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

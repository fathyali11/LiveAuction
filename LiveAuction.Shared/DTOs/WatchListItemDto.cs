namespace LiveAuction.Shared.DTOs;

public class WatchListItemDto
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal CurrentBid { get; set; }
    public DateTime AuctionStartTime { get; set; }
    public string ImageName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

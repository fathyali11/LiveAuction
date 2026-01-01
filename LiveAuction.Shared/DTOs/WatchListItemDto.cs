namespace LiveAuction.Shared.DTOs;

public class WatchListItemDto
{
    public int AuctionId { get; set; }
    public decimal CurrentBid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageName { get; set; }= string.Empty;
    public DateTime EndTime { get; set; }
    public bool IsInWatchList { get; set; }
}
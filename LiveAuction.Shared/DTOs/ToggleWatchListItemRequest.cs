namespace LiveAuction.Shared.DTOs;

public class ToggleWatchListItemRequest
{
    public int AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; } 
    public DateTime EndTime { get; set; }
}
public class ToggleWatchListItemResponse:ToggleWatchListItemRequest
{
    public bool IsInWatchList { get; set; }
}
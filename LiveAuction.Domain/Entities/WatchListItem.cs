namespace LiveAuction.Domain.Entities;

public class  WatchListItem
{
    public int Id { get; set; }
    public int WatchListId { get; set; }
    public WatchList WatchList { get; set; } = null!;
    public int AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal CurrentBid { get; set; }
    public DateTime AuctionEndTime { get; set; }
    public string ImageName { get; set; } = string.Empty;

}

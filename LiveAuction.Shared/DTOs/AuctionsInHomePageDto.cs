using LiveAuction.Shared.Enums;

namespace LiveAuction.Shared.DTOs;

public class AuctionsInHomePageDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public decimal CurrentBid { get; set; }
    public AuctionStatus Status { get; set; }
    public bool IsWatchListed { get; set; }
    public DateTime EndTime { get; set; }

}
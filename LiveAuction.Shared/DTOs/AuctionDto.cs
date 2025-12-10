using LiveAuction.Shared.Enums;

namespace LiveAuction.Shared.DTOs;

public class AuctionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } 
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal StartingBid { get; set; }
    public decimal? CurrentBid { get; set; }
    public string? CurrentBidder { get; set; }
    public string Seller { get; set; } = string.Empty;
    public AuctionStatus Status { get; set; }
    public bool IsActive => Status == AuctionStatus.Open&& DateTime.UtcNow >= StartTime && DateTime.UtcNow <= EndTime;
    public TimeSpan? TimeRemaining
    {
        get
        {
            if (Status != AuctionStatus.Open)
                return null;
            var remaining = EndTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
}

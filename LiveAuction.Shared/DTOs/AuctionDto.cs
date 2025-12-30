using LiveAuction.Shared.Enums;
using Microsoft.AspNetCore.Http;

namespace LiveAuction.Shared.DTOs;

public class AuctionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal StartingBid { get; set; }
    public decimal? CurrentBid { get; set; }
    public string? CurrentBidder { get; set; }
    public string Seller { get; set; } = string.Empty;
    public AuctionStatus Status { get; set; }
    public List<BidDto> Bids { get; set; } = [];
    public bool IsWatchListed { get; set; }
    public bool IsActive => Status == AuctionStatus.Open&& DateTime.UtcNow >= StartTime && DateTime.UtcNow <= EndTime;

}

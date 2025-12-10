namespace LiveAuction.Shared.DTOs;

public class CreateAuctionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime EndTime { get; set; }
    public decimal StartingBid { get; set; }
    public string Seller { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }

}

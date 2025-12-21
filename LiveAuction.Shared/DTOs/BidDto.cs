namespace LiveAuction.Shared.DTOs;

public class BidDto
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public decimal Amount { get; set; }
    public string Bidder { get; set; } = string.Empty;
    public DateTime TimePlaced { get; set; }
}

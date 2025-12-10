namespace LiveAuction.Domain.Entities;

public class Bid
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int AuctionId { get; set; }
    public string BidderId { get; set; }= string.Empty;
    public DateTime BidTime { get; set; }= DateTime.UtcNow;
    public ApplicationUser Bidder { get; set; } = null!;
    public Auction Auction { get; set; } = null!;
}
namespace LiveAuction.Shared.DTOs;

public class CreateBidRequest
{
    public int AuctionId { get; set; }
    public decimal Amount { get; set; }
}
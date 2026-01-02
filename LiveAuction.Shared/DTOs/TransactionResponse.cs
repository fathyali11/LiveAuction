namespace LiveAuction.Shared.DTOs;

public class TransactionResponse
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ?AuctionName { get; set; } 
    public int ?AuctionId { get; set; }
}
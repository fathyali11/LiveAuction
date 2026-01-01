using LiveAuction.Domain.Consts;

namespace LiveAuction.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreateAt { get; set; }=DateTime.UtcNow;
    public int? AuctionId { get; set; }
    public Auction? Auction { get; set; } 
}

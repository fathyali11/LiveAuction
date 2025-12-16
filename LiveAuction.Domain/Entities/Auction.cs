using LiveAuction.Shared.Enums;

namespace LiveAuction.Domain.Entities;

public class Auction
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; }= string.Empty;
    public string ImageName { get; set; }= string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public decimal StartingBid { get; set; }
    public decimal ?CurrentBid { get; set; }
    public string ?CurrentBidderId { get; set; }


    public string CreatedById { get; set; }= string.Empty;
    public string? WinnerId { get; set; }

    public AuctionStatus Status { get; set; }= AuctionStatus.Pending;
    public List<Bid> Bids { get; set; } = [];

    public ApplicationUser CreatedBy { get; set; } = null!;
    public ApplicationUser? Winner { get; set; }
    public ApplicationUser? CurrentBidder { get; set; }
}

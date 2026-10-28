namespace LiveAuction.Domain.Consts; 

/// <summary>
/// Result of the atomic auction update. The <c>Previous*</c> values come from the row as it was
/// before the update, so callers never have to rely on a stale pre-read.
/// </summary>
public class AuctionUpdateResult
{
    public int Id { get; set; }
    public decimal CurrentBid { get; set; }
    public DateTime EndTime { get; set; }
    public string? CurrentBidderId { get; set; }
    public string? JobId { get; set; }
    public int ExtensionCount { get; set; }
    public string? PreviousBidderId { get; set; }
    public decimal PreviousBid { get; set; }
}

using LiveAuction.Shared.Enums;

namespace LiveAuction.Shared.DTOs;

/// <summary>
/// Lightweight, read-only snapshot used to validate a bid without loading the full auction graph.
/// The authoritative checks are still performed atomically inside <c>TryPlaceBidAsync</c>.
/// </summary>
public record AuctionBidContext(
    int Id,
    string Title,
    string CreatedById,
    AuctionStatus Status,
    DateTime StartTime,
    DateTime EndTime,
    decimal CurrentBid,
    string? CurrentBidderId,
    string? JobId,
    int ExtensionCount);

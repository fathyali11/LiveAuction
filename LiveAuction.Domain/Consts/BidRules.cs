namespace LiveAuction.Domain.Consts;

/// <summary>
/// Server-side bidding rules. Keep these values authoritative; clients must never decide them.
/// </summary>
public static class BidRules
{
    /// <summary>Minimum amount a new bid must exceed the current bid by.</summary>
    public const decimal MinimumIncrement = 1m;

    /// <summary>Upper bound accepted for a single bid. Matches decimal(18,2) safely.</summary>
    public const decimal MaximumAmount = 1_000_000_000m;

    public const int IdempotencyKeyMaxLength = 100;

    /// <summary>Bids placed within this window before the end extend the auction.</summary>
    public const int SnipingWindowMinutes = 5;

    /// <summary>How long a late bid extends the auction.</summary>
    public const int ExtensionMinutes = 1;

    /// <summary>Hard cap on automatic extensions, so an auction cannot be kept open forever.</summary>
    public const int MaxExtensions = 30;
}

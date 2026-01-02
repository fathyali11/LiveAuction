namespace LiveAuction.Shared.DTOs;

public class WalletSummaryResponse
{
    public decimal TotalBalance { get; set; }
    public decimal LockedBalance { get; set; }
    public decimal AvailableBalance => TotalBalance - LockedBalance;
    public List<TransactionResponse> Transactions { get; set; } = [];
}

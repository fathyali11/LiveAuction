using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Domain.Repositories;

public interface IWalletRepository
{
    Task<ApplicationUser?> GetUserWithBalanceAsync(string userId);
    Task AddTransactionAsync(Transaction transaction,CancellationToken cancellationToken=default);
    Task AddTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default);
    Task<(IDbContextTransaction, bool current)> BeginTransaction(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<WalletSummaryResponse?> GetWalletSummaryAsync(string userId, CancellationToken cancellationToken = default);
    Task<(List<TransactionResponse> transactions, int count)> GetTransactionsAndItsCountAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken = default);
    Task<bool> HoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken = default);
    Task<bool> DepositAsync(string userId, decimal amount, CancellationToken cancellationToken = default);
    Task<bool> ReleaseHoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken = default);
    Task<bool> WithdrawFromWinnerAsync(string winnerId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the net amount currently held for a user on a specific auction
    /// (sum of holds minus sum of releases). Returns zero when there is no active hold.
    /// </summary>
    Task<decimal> GetActiveHoldAmountAsync(string userId, int auctionId, CancellationToken cancellationToken = default);

}

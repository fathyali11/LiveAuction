using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Domain.Repositories;

public interface IWalletRepository
{
    Task<ApplicationUser?> GetUserWithBalanceAsync(string userId);
    Task AddTransactionAsync(Transaction transaction,CancellationToken cancellationToken=default);
    Task AddTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<WalletSummaryResponse?> GetWalletSummaryAsync(string userId, CancellationToken cancellationToken = default);
}

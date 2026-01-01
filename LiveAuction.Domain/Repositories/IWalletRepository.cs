using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Domain.Repositories;

public interface IWalletRepository
{
    Task<ApplicationUser?> GetUserWithBalanceAsync(string userId);
    Task AddTransactionAsync(Transaction transaction,CancellationToken cancellationToken=default);
    Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

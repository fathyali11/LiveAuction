using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Infrastructure.Repositories;

internal class WalletRepository(ApplicationDbContext _context) : IWalletRepository
{
    public async Task AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction,cancellationToken);
        await _context.Transactions.AddRangeAsync(new[] { transaction }, cancellationToken);
    }
    public async Task AddTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddRangeAsync(transactions, cancellationToken);
    }
    public async Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<ApplicationUser?> GetUserWithBalanceAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<WalletSummaryResponse?> GetWalletSummaryAsync(string userId,CancellationToken cancellationToken=default)
    {
        var walletResponse = await _context.Users
            .AsNoTracking()
            .Include(x=> x.Transactions)
            .Where(u => u.Id == userId)
            .Select(u => new WalletSummaryResponse
            {
                TotalBalance = u.TotalBalance,
                LockedBalance = u.LockedBalance,
                Transactions = u.Transactions
                    .OrderByDescending(t => t.CreateAt)
                    .Select(t => new TransactionResponse
                    {
                        TransactionId = t.Id,
                        Amount = t.Amount,
                        Timestamp = t.CreateAt,
                        Type = t.TransactionType.ToString()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        return walletResponse;
    }
    
    public async Task<List<TransactionResponse>> GetTransactionsAsync(string userId,int pageNumber,int pageSize,CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreateAt);

        var count = await query.CountAsync(cancellationToken);

        var transactions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionResponse
            {
                TransactionId = t.Id,
                Amount = t.Amount,
                Timestamp = t.CreateAt,
                Type = t.TransactionType.ToString(),
                AuctionId = t.AuctionId,
                AuctionName = t.AuctionId != null ? t.Auction!.Title : null
            })
            .ToListAsync(cancellationToken);

        return transactions;
    }
}

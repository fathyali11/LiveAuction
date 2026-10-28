using System.Data;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Infrastructure.Repositories;

internal class WalletRepository(ApplicationDbContext _context) : IWalletRepository
{
    public async Task AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }
    public async Task AddTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddRangeAsync(transactions, cancellationToken);
    }
    public async Task<(IDbContextTransaction, bool current)> BeginTransaction(CancellationToken cancellationToken = default)
    {
        var currentTransaction = _context.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            return (currentTransaction, true);
        }
        return (await _context.Database.BeginTransactionAsync(cancellationToken), false);
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
            .Where(u => u.Id == userId)
            .Select(u => new WalletSummaryResponse
            {
                TotalBalance = u.TotalBalance,
                LockedBalance = u.LockedBalance
            })
            .FirstOrDefaultAsync(cancellationToken);
        return walletResponse;
    }
    
    public async Task<(List<TransactionResponse> transactions, int count)> GetTransactionsAndItsCountAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if(!string.IsNullOrWhiteSpace(paginatedRequest.SearchTerm))
        {
            query = query.Where(t =>
                t.TransactionType.ToString().Contains(paginatedRequest.SearchTerm) ||
                (t.Auction != null && t.Auction.Title.Contains(paginatedRequest.SearchTerm))
            );
        }

        var count = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.CreateAt)
            .Skip((paginatedRequest.PageNumber - 1) * paginatedRequest.PageSize)
            .Take(paginatedRequest.PageSize)
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

        return (transactions,count);
    }

    public async Task<bool> HoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _context.Users
            .Where(u => u.Id == userId && (u.TotalBalance - u.LockedBalance) >= amount)
            .ExecuteUpdateAsync(u => u.SetProperty(user => user.LockedBalance, user => user.LockedBalance + amount), cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DepositAsync(string userId, decimal amount, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(user => user.TotalBalance, user => user.TotalBalance + amount), cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> ReleaseHoldAsync(string userId, decimal amount, int auctionId, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return false;
        }

        // The release is only allowed against the net amount held for this specific auction.
        // LockedBalance is a single pooled column, so without this guard a release for auction A
        // could free funds that belong to auction B.
        const string sql = @"
            UPDATE AspNetUsers
            SET LockedBalance = LockedBalance - @amount
            WHERE Id = @userId
              AND LockedBalance >= @amount
              AND @amount <= (
                  SELECT ISNULL(SUM(CASE
                      WHEN t.TransactionType = @holdType THEN t.Amount
                      ELSE -t.Amount
                  END), 0)
                  FROM Transactions t
                  WHERE t.UserId = @userId
                    AND t.AuctionId = @auctionId
                    AND t.TransactionType IN (@holdType, @releaseType)
              );";

        var parameters = new[]
        {
            new SqlParameter("@amount", SqlDbType.Decimal) { Value = amount, Precision = 18, Scale = 2 },
            new SqlParameter("@userId", SqlDbType.NVarChar, 450) { Value = userId },
            new SqlParameter("@auctionId", SqlDbType.Int) { Value = auctionId },
            new SqlParameter("@holdType", SqlDbType.Int) { Value = (int)TransactionType.Hold },
            new SqlParameter("@releaseType", SqlDbType.Int) { Value = (int)TransactionType.Release }
        };

        var affectedRows = await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> WithdrawFromWinnerAsync(string winnerId, decimal amount, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _context.Users
            .Where(u => u.Id == winnerId && u.TotalBalance >= amount && u.LockedBalance >= amount)
            .ExecuteUpdateAsync(u => 
            u.SetProperty(user => user.TotalBalance, user => user.TotalBalance - amount)
            .SetProperty(user => user.LockedBalance, user => user.LockedBalance - amount)
            , cancellationToken);
        return affectedRows > 0;
    }
    public async Task<decimal> GetActiveHoldAmountAsync(string userId, int auctionId, CancellationToken cancellationToken = default)
    {
        var net = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId &&
                        t.AuctionId == auctionId &&
                        (t.TransactionType == TransactionType.Hold ||
                         t.TransactionType == TransactionType.Release))
            .Select(t => (decimal?)(t.TransactionType == TransactionType.Hold ? t.Amount : -t.Amount))
            .SumAsync(cancellationToken) ?? 0m;

        return net > 0 ? net : 0m;
    }

}

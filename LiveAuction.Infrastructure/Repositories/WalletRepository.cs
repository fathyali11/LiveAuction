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
        await _context.Transactions.AddRangeAsync([transaction], cancellationToken);
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
}

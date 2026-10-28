using System.Data;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Infrastructure.Repositories;

internal class AuctionRepository(ApplicationDbContext _context) : IAuctionRepository
{
    public async Task AddAsync(Auction auction, CancellationToken cancellationToken)
    {
        await _context.Auctions.AddAsync(auction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<AuctionsInHomePageDto> auctions, int count)>GetAllActiveAndItsCountAsync(
    string? userId,
    PaginatedRequest paginatedRequest,
    CancellationToken cancellationToken)
    {
        paginatedRequest.PageNumber = paginatedRequest.PageNumber <= 0 ? 1 : paginatedRequest.PageNumber;
        paginatedRequest.PageSize = paginatedRequest.PageSize <= 0 ? 10 : paginatedRequest.PageSize;

        var now = DateTime.UtcNow;

        var query = _context.Auctions
            .AsNoTracking()
            .Where(a =>
                a.Status == AuctionStatus.Open &&
                a.StartTime <= now &&
                a.EndTime > now);

        if (!string.IsNullOrWhiteSpace(paginatedRequest.SearchTerm))
        {
            query = query.Where(a =>
                a.Title.Contains(paginatedRequest.SearchTerm) ||
                a.Description.Contains(paginatedRequest.SearchTerm));
        }

        var count = await query.CountAsync(cancellationToken);

        var auctions = await query
            .OrderByDescending(a => a.StartTime)
            .Skip((paginatedRequest.PageNumber - 1) * paginatedRequest.PageSize)
            .Take(paginatedRequest.PageSize)
            .Select(a => new AuctionsInHomePageDto
            {
                Id = a.Id,
                Title = a.Title,
                ImageName = a.ImageName,
                CurrentBid = a.CurrentBid,
                Status = a.Status,
                EndTime = a.EndTime,
                IsWatchListed = userId != null &&
                    _context.WatchListItems
                        .Any(w => w.AuctionId == a.Id && w.WatchList.UserId == userId)
            })
            .ToListAsync(cancellationToken);

        return (auctions, count);
    }


    public async Task<Auction?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await _context.Auctions
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Auction?> GetByIdWithBidsAsync(int id, CancellationToken cancellationToken)
        => await _context.Auctions
            .Include(a => a.CreatedBy)
            .Include(a => a.Winner)
            .Include(a => a.Bids)
            .ThenInclude(b => b.Bidder)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<AuctionBidContext?> GetBidContextAsync(int id, CancellationToken cancellationToken)
        => await _context.Auctions
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AuctionBidContext(
                a.Id,
                a.Title,
                a.CreatedById,
                a.Status,
                a.StartTime,
                a.EndTime,
                a.CurrentBid,
                a.CurrentBidderId,
                a.JobId,
                a.ExtensionCount))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> AcquireAuctionLockAsync(int auctionId, CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = @"
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = @resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;
            SELECT @result;";

        var resourceParameter = command.CreateParameter();
        resourceParameter.ParameterName = "@resource";
        resourceParameter.DbType = DbType.String;
        resourceParameter.Value = $"auction-bid:{auctionId}";
        command.Parameters.Add(resourceParameter);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is int lockResult && lockResult >= 0;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions.FindAsync([id], cancellationToken);
        if (auction != null)
        {
            _context.Auctions.Remove(auction);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }



    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Auction?> GetAuctionToTerminateAsync(int auctionId, CancellationToken cancellationToken)
     => await _context.Auctions
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == auctionId, cancellationToken);

    public async Task<AuctionUpdateResult?> TryPlaceBidAsync(
    int auctionId,
    string userId,
    decimal amount,
    CancellationToken cancellationToken)
    {
        // All time checks and the anti-sniping extension use the database clock, so multiple
        // application servers cannot disagree about whether the auction is still open.
        var sql = @"
            UPDATE Auctions 
            SET CurrentBid = @Amount,
                CurrentBidderId = @UserId,
                UpdatedAt = SYSUTCDATETIME(),
                ExtensionCount = CASE
                    WHEN EndTime <= DATEADD(MINUTE, @WindowMinutes, SYSUTCDATETIME())
                         AND ExtensionCount < @MaxExtensions
                    THEN ExtensionCount + 1
                    ELSE ExtensionCount
                END,
                EndTime = CASE 
                    WHEN EndTime <= DATEADD(MINUTE, @WindowMinutes, SYSUTCDATETIME())
                         AND ExtensionCount < @MaxExtensions
                    THEN DATEADD(MINUTE, @ExtensionMinutes, EndTime)
                    ELSE EndTime
                END
            OUTPUT DELETED.CurrentBidderId AS PreviousBidderId,
                DELETED.CurrentBid AS PreviousBid,
                INSERTED.Id, INSERTED.CurrentBid, INSERTED.EndTime, 
                INSERTED.CurrentBidderId, INSERTED.JobId, INSERTED.ExtensionCount
            WHERE Id = @Id
            AND Status = @OpenStatus
            AND StartTime <= SYSUTCDATETIME()
            AND EndTime > SYSUTCDATETIME()
            AND CurrentBid + @MinimumIncrement <= @Amount;";

        var parameters = new[]
        {
            new SqlParameter("@Amount", SqlDbType.Decimal) { Value = amount, Precision = 18, Scale = 2 },
            new SqlParameter("@UserId", SqlDbType.NVarChar, 450) { Value = userId },
            new SqlParameter("@Id", SqlDbType.Int) { Value = auctionId },
            new SqlParameter("@OpenStatus", SqlDbType.Int) { Value = (int)AuctionStatus.Open },
            new SqlParameter("@MinimumIncrement", SqlDbType.Decimal) { Value = BidRules.MinimumIncrement, Precision = 18, Scale = 2 },
            new SqlParameter("@WindowMinutes", SqlDbType.Int) { Value = BidRules.SnipingWindowMinutes },
            new SqlParameter("@ExtensionMinutes", SqlDbType.Int) { Value = BidRules.ExtensionMinutes },
            new SqlParameter("@MaxExtensions", SqlDbType.Int) { Value = BidRules.MaxExtensions }
        };

        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);

        await using var reader = await command.ExecuteReaderAsync(
            System.Data.CommandBehavior.SingleRow, cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var previousBidderOrdinal = reader.GetOrdinal("PreviousBidderId");
        return new AuctionUpdateResult
        {
            PreviousBidderId = reader.IsDBNull(previousBidderOrdinal)
                ? null
                : reader.GetString(previousBidderOrdinal),
            PreviousBid = reader.GetDecimal(reader.GetOrdinal("PreviousBid")),
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CurrentBid = reader.GetDecimal(reader.GetOrdinal("CurrentBid")),
            EndTime = reader.GetDateTime(reader.GetOrdinal("EndTime")),
            CurrentBidderId = reader.IsDBNull(reader.GetOrdinal("CurrentBidderId"))
                ? null
                : reader.GetString(reader.GetOrdinal("CurrentBidderId")),
            JobId = reader.IsDBNull(reader.GetOrdinal("JobId"))
                ? null
                : reader.GetString(reader.GetOrdinal("JobId")),
            ExtensionCount = reader.GetInt32(reader.GetOrdinal("ExtensionCount"))
        };
    }

    public async Task UpdateJobIdAsync(int auctionId, string jobId, CancellationToken cancellationToken)
    {
        await _context.Auctions
            .Where(a => a.Id == auctionId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(a => a.JobId, jobId),
                cancellationToken);
    }
}

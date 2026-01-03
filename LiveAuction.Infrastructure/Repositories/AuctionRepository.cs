using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LiveAuction.Infrastructure.Repositories;

internal class AuctionRepository(ApplicationDbContext _context,
    IServiceProvider _serviceProvider) : IAuctionRepository
{
    public async Task AddAsync(Auction auction, CancellationToken cancellationToken)
    {
        await _context.Auctions.AddAsync(auction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AuctionsInHomePageDto>> GetAllActiveAsync(string ?userId,CancellationToken cancellationToken)
    {
        var auctions = await _context.Auctions
            .Where(a => 
            a.Status == AuctionStatus.Open
            && a.EndTime > DateTime.UtcNow
            )
            .Select(a => new AuctionsInHomePageDto
            {
                Id = a.Id,
                Title = a.Title,
                ImageName = a.ImageName,
                CurrentBid = a.CurrentBid!.Value,
                Status = a.Status,
                EndTime = a.EndTime,
                IsWatchListed = _context.WatchListItems
                    .Any(w => w.AuctionId == a.Id&&w.WatchList.UserId==userId)
            })
            .ToListAsync(cancellationToken);

        return auctions;

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

    public async Task UpdateAsync(Auction auction, CancellationToken cancellationToken)
    {
        _context.Auctions.Update(auction);
        await _context.SaveChangesAsync(cancellationToken);
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
}

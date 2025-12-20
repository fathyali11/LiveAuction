using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Repositories;

internal class AuctionRepository(ApplicationDbContext _context) : IAuctionRepository
{
    public async Task AddAsync(Auction auction, CancellationToken cancellationToken)
    {
        await _context.Auctions.AddAsync(auction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Auction>> GetAllActiveAsync(CancellationToken cancellationToken)
        => await _context.Auctions
            .Include(a => a.CreatedBy)
            .Where(a => a.Status == AuctionStatus.Open)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<Auction?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await _context.Auctions
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Auction?> GetByIdWithBidsAsync(int id, CancellationToken cancellationToken)
        => await _context.Auctions
            .Include(a => a.CreatedBy)
            .Include(a => a.Bids)
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

    public async Task<bool> AddCurrentBidAsync(int auctionId, decimal amount, CancellationToken cancellationToken)
    {
        var added = await _context.Auctions
            .Where(a => a.Id == auctionId)
            .ExecuteUpdateAsync(a => a.SetProperty(a => a.CurrentBid, amount), cancellationToken);
        return added > 0;
    }
}

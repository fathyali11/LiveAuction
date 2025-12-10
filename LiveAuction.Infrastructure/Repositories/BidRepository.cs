using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Repositories;

internal class BidRepository(ApplicationDbContext _context) : IBidRepository
{
    public async Task AddAsync(Bid bid,CancellationToken cancellationToken)
    {
        await _context.Bids.AddAsync(bid,cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Bid>> GetHistoryAsync(int auctionId,CancellationToken cancellationToken)
        => await _context.Bids.
            Where(x => x.AuctionId == auctionId)
            .Include(x => x.Bidder)
            .OrderByDescending(x => x.BidTime)
            .ToListAsync(cancellationToken);
}

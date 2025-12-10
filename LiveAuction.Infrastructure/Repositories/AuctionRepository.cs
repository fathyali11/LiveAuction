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
            .Where(a => a.Status == AuctionStatus.Open)
            .ToListAsync(cancellationToken);

    public async Task<Auction?> GetByIdAsync(int id,CancellationToken cancellationToken)
        => await _context.Auctions
            .FindAsync([id], cancellationToken);

    public async Task UpdateAsync(Auction auction,CancellationToken cancellationToken)
    {
        _context.Auctions.Update(auction);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

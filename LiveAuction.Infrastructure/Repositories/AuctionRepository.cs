using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Repositories;

internal class AuctionRepository(ApplicationDbContext _context) : IAuctionRepository
{
    public async Task AddAsync(Auction auction)
    {
        await _context.Auctions.AddAsync(auction);
        await _context.SaveChangesAsync();
    }

    public Task<List<Auction>> GetAllActiveAsync()
    {
        var auctions = _context.Auctions
            .Where(a => a.Status == AuctionStatus.Open)
            .ToListAsync();
        return auctions;
    }

    public Task<Auction?> GetByIdAsync(int id)
    {
        var auction = _context.Auctions
            .FirstOrDefaultAsync(a => a.Id == id);
        return auction;
    }

    public async Task UpdateAsync(Auction auction)
    {
        _context.Auctions.Update(auction);
        await _context.SaveChangesAsync();
    }
}

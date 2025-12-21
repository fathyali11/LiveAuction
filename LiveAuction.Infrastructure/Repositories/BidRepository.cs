using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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

    public async Task<List<UserBidDto>> GetUserBidsHistoryAsync(string userId,CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var bids = await _context.Bids
            .AsNoTracking()
            .Include(b => b.Auction)
            .Where(b => b.BidderId == userId)
            .ToListAsync(cancellationToken);

        var userBids = bids
            .GroupBy(x => x.AuctionId)
            .Select(group =>
            {
                var auction = group.First().Auction;
                var myHighestBid = group.Max(b => b.Amount);
                var currentAuctionBid = auction.CurrentBid!.Value;
                string status;
                if (now > auction.EndTime)
                    status = myHighestBid >= currentAuctionBid ? BidStatus.Won.ToString() : BidStatus.Lost.ToString();
                else
                    status = myHighestBid >= currentAuctionBid ? BidStatus.Winning.ToString() : BidStatus.Outbid.ToString();

                return new UserBidDto
                {
                    BidId = group.First().Id,
                    AuctionId = auction.Id,
                    Title = auction.Title,
                    ImageName = auction.ImageName,
                    MyHighestBid = myHighestBid,
                    CurrentHighBid = currentAuctionBid,
                    AuctionEndTime = auction.EndTime,
                    Status = status
                };
            });
        return userBids.ToList();
    }
}

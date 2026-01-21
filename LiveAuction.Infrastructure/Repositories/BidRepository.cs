using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<List<UserBidDto>> GetUserBidsHistoryAsync(string userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var userBidsData = await _context.Bids
            .AsNoTracking()
            .Where(b => b.BidderId == userId)
            .GroupBy(b => b.AuctionId)  
            .Select(g => new
            {
                Auction = g.First().Auction,

                MyHighestBid = g.Max(b => b.Amount),

                LatestBidId = g.OrderByDescending(b => b.Amount).First().Id
            })
            .ToListAsync(cancellationToken);  

        var result = userBidsData.Select(item =>
        {
            var auction = item.Auction;
            var myHighestBid = item.MyHighestBid;
            var currentAuctionBid = auction.CurrentBid;

            string status;

            if (now > auction.EndTime)
            {
                status = myHighestBid >= currentAuctionBid ? BidStatus.Won : BidStatus.Lost;
            }
            else
            {
                status = myHighestBid >= currentAuctionBid ? BidStatus.Winning : BidStatus.Outbid;
            }

            return new UserBidDto
            {
                BidId = item.LatestBidId,
                AuctionId = auction.Id,
                Title = auction.Title,
                ImageName = auction.ImageName,
                MyHighestBid = myHighestBid,
                CurrentHighBid = currentAuctionBid,
                AuctionEndTime = auction.EndTime,
                Status = status 
            };
        }).ToList();

        return result;
    }



    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => await _context.Database.BeginTransactionAsync(cancellationToken);
}

using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Repositories;

internal class WatchListRepository(ApplicationDbContext _context) : IWatchListRepository
{
    public async Task<bool> ToggleAndReturnCurrentStateOfExitanceAsync(string userId, Auction auction, CancellationToken cancellationToken = default)
    {
        var watchList = await _context.WatchLists
            .Include(wl => wl.Items)
            .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);
        if (watchList == null)
        {
            watchList = new WatchList
            {
                UserId = userId,
                Items = []
            };
            await _context.WatchLists.AddAsync(watchList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        var existingItem = watchList.Items.FirstOrDefault(item => item.AuctionId == auction.Id);
        if (existingItem != null)
        {
            watchList.Items.Remove(existingItem);
            _context.WatchListItems.Remove(existingItem);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
        else
        {
            var newItem = new WatchListItem
            {
                AuctionId = auction.Id,
                WatchListId = watchList.Id,
                CurrentBid = auction.CurrentBid,
                Title = auction.Title,
                ImageName = auction.ImageName,
                EndTime = auction.EndTime
            };
            watchList.Items.Add(newItem);
            await _context.WatchListItems.AddAsync(newItem, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
    public async Task ClearAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _context.WatchListItems
            .Where(item => item.WatchList.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
    public async Task<int> GetCountAsync(string ?userId, CancellationToken cancellationToken = default)
    {
        if(userId is null) return 0;
        var count = await _context.WatchListItems
            .Where(item => item.WatchList.UserId == userId)
            .CountAsync(cancellationToken);
        return count;
    }

    public async Task<(List<WatchListItemDto> items,int count)> GetWatchListItemAndItsCountAsync(string userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken = default)
    {
        var query = _context.WatchListItems
            .Where(item => item.WatchList.UserId == userId);
        if(!string.IsNullOrWhiteSpace(paginatedRequest.SearchTerm))
        {
            var searchTerm = paginatedRequest.SearchTerm.Trim().ToLower();
            query = query.Where(item => item.Title.ToLower().Contains(searchTerm));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(item => item.Id)
            .Skip((paginatedRequest.PageNumber - 1) * paginatedRequest.PageSize)
            .Take(paginatedRequest.PageSize)
            .Select(item => new WatchListItemDto
            {
                AuctionId = item.AuctionId,
                CurrentBid = item.CurrentBid,
                Title = item.Title,
                ImageName = item.ImageName,
                EndTime = item.EndTime,
                IsInWatchList = true
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
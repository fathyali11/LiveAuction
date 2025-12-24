using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Repositories;

internal class WatchListRepository(ApplicationDbContext _context) : IWatchListRepository
{
    public async Task<bool> ToggleAndReturnCurrentStateOfExitanceAsync(string userId, ToggleWatchListItemRequest request, CancellationToken cancellationToken = default)
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
        var existingItem = watchList.Items.FirstOrDefault(item => item.AuctionId == request.AuctionId);
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
                AuctionId = request.AuctionId,
                WatchListId = watchList.Id,
                CurrentBid = request.CurrentPrice,
                Title = request.Title,
                ImageName = request.ImageName,
                EndTime = request.EndTime
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
    public async Task<WatchListDto> GetWatchListItems(string userId, CancellationToken cancellationToken = default)
    {
        var watchList = await _context.WatchLists
            .Include(wl => wl.Items)
            .FirstOrDefaultAsync(x=>x.UserId==userId, cancellationToken);
        if (watchList is null)
        {
            return new WatchListDto
            {
                UserId = userId,
                Items = []
            };
        }
        return new WatchListDto
        {
            Id = watchList.Id,
            UserId = watchList.UserId,
            Items = watchList.Items.Select(item => new ToggleWatchListItemResponse
            {
                AuctionId = item.AuctionId,
                CurrentPrice = item.CurrentBid,
                Title = item.Title,
                ImageName = item.ImageName,
                EndTime = item.EndTime,
                IsInWatchList = true
            }).ToList()
        };
    }
    public async Task<int> GetCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        var count = await _context.WatchListItems
            .Where(item => item.WatchList.UserId == userId)
            .CountAsync(cancellationToken);
        return count;
    }
}
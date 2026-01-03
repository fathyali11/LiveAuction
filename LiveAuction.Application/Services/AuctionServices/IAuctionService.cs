using LiveAuction.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LiveAuction.Application.Services.AuctionServices;

public interface IAuctionService
{
    Task<string> SaveImageAsync(IFormFile image, CancellationToken cancellationToken);
    Task DeleteImageAsync(string imageName, CancellationToken cancellationToken);
    Task<string> ScheduleAuction(Auction auction, CancellationToken cancellationToken);
    Task TerminateAuctionAsync(int auctionId, CancellationToken cancellationToken);
}

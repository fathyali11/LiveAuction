using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;
using Microsoft.AspNetCore.Http;

namespace LiveAuction.Application.Services.AuctionServices;

public interface IAuctionService
{
    Task<PaginatedResult<AuctionsInHomePageDto>> GetAllActiveAuctionsAsync(string? userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<string> SaveImageAsync(IFormFile image, CancellationToken cancellationToken);
    Task DeleteImageAsync(string imageName, CancellationToken cancellationToken);
    Task<string> ScheduleAuction(Auction auction, CancellationToken cancellationToken);
    Task TerminateAuctionAsync(int auctionId, CancellationToken cancellationToken);
}

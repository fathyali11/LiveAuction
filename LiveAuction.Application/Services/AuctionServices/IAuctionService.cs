using LiveAuction.Domain.Entities;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.Services.AuctionServices;

public interface IAuctionService
{
    Task<PaginatedResult<AuctionsInHomePageDto>> GetAllActiveAuctionsAsync(string? userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken);
    Task<string> ScheduleAuction(Auction auction, CancellationToken cancellationToken);
    Task CancelScheduledAuction(string jobId, CancellationToken cancellationToken);
    Task TerminateAuctionAsync(int auctionId, CancellationToken cancellationToken);
}

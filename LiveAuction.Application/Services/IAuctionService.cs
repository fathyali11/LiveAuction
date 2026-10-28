using LiveAuction.Shared.DTOs;

namespace LiveAuction.Application.Services;

public interface IAuctionService
{
    Task<PaginatedResult<AuctionsInHomePageDto>> GetAllActiveAuctionsAsync(string? userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken);

    /// <summary>Schedules the termination job for a specific auction end time.</summary>
    Task<string> ScheduleAuction(int auctionId, DateTime endTime, CancellationToken cancellationToken);

    Task CancelScheduledAuction(string jobId, CancellationToken cancellationToken);
    Task TerminateAuctionAsync(int auctionId, CancellationToken cancellationToken);
}

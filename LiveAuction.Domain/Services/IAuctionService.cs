using Microsoft.AspNetCore.Http;

namespace LiveAuction.Domain.Services;

public interface IAuctionService
{
    Task<string> SaveImageAsync(IFormFile image, CancellationToken cancellationToken);
}

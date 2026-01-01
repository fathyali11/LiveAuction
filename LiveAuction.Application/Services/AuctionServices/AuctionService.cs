using LiveAuction.Application.Services.BackgroundJobServices;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace LiveAuction.Application.Services.AuctionServices;

internal class AuctionService(IBackgroundJobService _backgroundJobService): IAuctionService
{
    public async Task<string> SaveImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        var imageName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var imageFolder = Path.Combine("wwwroot", "images");
        if (!Directory.Exists(imageFolder))
        {
            Directory.CreateDirectory(imageFolder);
        }
        var imagePath = Path.Combine(imageFolder, imageName);
        using var stream = new FileStream(imagePath, FileMode.Create);
        await image.CopyToAsync(stream, cancellationToken);

        return imageName;
    }
    public async Task DeleteImageAsync(string imageName, CancellationToken cancellationToken)
    {
        var imagePath = Path.Combine("wwwroot", "images", imageName);
        if (File.Exists(imagePath))
        {
            await Task.Run(() => File.Delete(imagePath), cancellationToken);
        }
    }

    public async Task<string> ScheduleAuction(Auction auction,CancellationToken cancellationToken)
    {
        var jobId = _backgroundJobService.ScheduleJob<IAuctionRepository>(
            repo => repo.TerminateAuctionAsync(auction.Id, cancellationToken),
            auction.EndTime - DateTime.UtcNow
            );
        return jobId;
    }


}

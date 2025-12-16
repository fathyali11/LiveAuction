using LiveAuction.Domain.Services;
using Microsoft.AspNetCore.Http;

namespace LiveAuction.Infrastructure.Services;

internal class AuctionService: IAuctionService
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
}

using Microsoft.AspNetCore.Http;

namespace LiveAuction.Application.Interfaces;

public interface IFileService
{
    Task<string?> SaveFileAsync(IFormFile file, CancellationToken cancellationToken);
    Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken);
    
}
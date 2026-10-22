using LiveAuction.Application.Common.Validation;
using LiveAuction.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Infrastructure.Services;

internal class LocalFileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<LocalFileService> _logger;

    private string _imagesFolderPath =>
        Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot"), "images");

    public LocalFileService(IWebHostEnvironment webHostEnvironment, ILogger<LocalFileService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;

        if (!Directory.Exists(_imagesFolderPath))
        {
            Directory.CreateDirectory(_imagesFolderPath);
            _logger.LogInformation("Created images directory at: {Path}", _imagesFolderPath);
        }
    }

    public async Task<string?> SaveFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("Attempted to save a null or empty file.");
            return null;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageFileValidation.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Attempted to save a file with an unsupported extension: {Extension}", extension);
            return null;
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_imagesFolderPath, uniqueFileName);

        try
        {
            await using var fileStream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await file.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation("File saved successfully: {FileName}", uniqueFileName);
            return uniqueFileName;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            TryDeleteFile(filePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while saving the file.");
            TryDeleteFile(filePath);
            return null;
        }
    }

    public Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            _logger.LogWarning("Attempted to delete a file with an invalid or empty file name.");
            return Task.FromResult(false);
        }

        var folderPath = Path.GetFullPath(_imagesFolderPath);
        var filePath = Path.GetFullPath(Path.Combine(folderPath, fileName));

        if (!filePath.StartsWith(folderPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Attempted to delete a file outside the images folder: {FileName}", fileName);
            return Task.FromResult(false);
        }

        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Attempted to delete non-existent file: {FileName}", fileName);
                return Task.FromResult(false);
            }

            File.Delete(filePath);
            _logger.LogInformation("File deleted successfully: {FileName}", fileName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FileName}", fileName);
            return Task.FromResult(false);
        }
    }

    private void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean up file: {FilePath}", filePath);
        }
    }
}

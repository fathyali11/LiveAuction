using Microsoft.AspNetCore.Http;

namespace LiveAuction.Application.Common.Validation;


public static class ImageFileValidation
{
    public const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    public static readonly IReadOnlyCollection<string> AllowedExtensions = [".jpg", ".jpeg", ".png"];

    public static readonly IReadOnlyCollection<string> AllowedContentTypes = ["image/jpeg", "image/png"];

    private static readonly Dictionary<string, byte[]> AllowedSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = [0xFF, 0xD8, 0xFF],
        [".jpeg"] = [0xFF, 0xD8, 0xFF],
        [".png"] = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]
    };

    public static bool HasAllowedSize(IFormFile? file)
        => file is not null && file.Length > 0 && file.Length <= MaxFileSizeInBytes;

    public static bool HasAllowedExtension(IFormFile? file)
        => file is not null && AllowedExtensions.Contains(GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase);

    public static bool HasAllowedContentType(IFormFile? file)
        => file is not null && AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);

    public static bool HasValidSignature(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return false;
        }

        var extension = GetExtension(file.FileName);
        if (!AllowedSignatures.TryGetValue(extension, out var expectedSignature))
        {
            return false;
        }

        using var stream = file.OpenReadStream();
        var header = new byte[expectedSignature.Length];
        var bytesRead = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);

        return bytesRead == header.Length && header.AsSpan().SequenceEqual(expectedSignature);
    }

    private static string GetExtension(string? fileName)
        => string.IsNullOrWhiteSpace(fileName) ? string.Empty : Path.GetExtension(fileName);
}

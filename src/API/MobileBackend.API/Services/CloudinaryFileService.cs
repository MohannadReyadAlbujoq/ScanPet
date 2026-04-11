using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MobileBackend.Application.Common.Interfaces;

namespace MobileBackend.API.Services;

/// <summary>
/// Cloudinary-based file service for persistent cloud image storage.
/// Replaces local FileService since Render.com has ephemeral filesystem.
/// Images are stored permanently on Cloudinary CDN.
/// </summary>
public class CloudinaryFileService : IFileService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryFileService> _logger;

    public CloudinaryFileService(IConfiguration configuration, ILogger<CloudinaryFileService> logger)
    {
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"]
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
        var apiKey = configuration["Cloudinary:ApiKey"]
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary credentials not configured. Set Cloudinary:CloudName, Cloudinary:ApiKey, Cloudinary:ApiSecret in appsettings.json " +
                "or CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET environment variables.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;

        _logger.LogInformation("Cloudinary file service initialized for cloud: {CloudName}", cloudName);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder, CancellationToken cancellationToken = default)
    {
        var publicId = $"scanpet/{subfolder}/{Guid.NewGuid()}";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            PublicId = publicId,
            Folder = $"scanpet/{subfolder}",
            Overwrite = true,
            Transformation = new Transformation()
                .Width(800).Height(800).Crop("limit") // Max 800x800, preserve aspect ratio
                .Quality("auto")
                .FetchFormat("auto") // Auto-select best format (webp, avif, etc.)
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");
        }

        _logger.LogInformation("Image uploaded to Cloudinary: {Url} (PublicId: {PublicId})", result.SecureUrl, result.PublicId);

        // Return the full HTTPS URL — no need for local URL resolution
        return result.SecureUrl.ToString();
    }

    public void DeleteFile(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        // Only delete Cloudinary URLs
        if (!imageUrl.Contains("cloudinary.com")) return;

        try
        {
            // Extract public ID from URL
            // URL format: https://res.cloudinary.com/{cloud}/image/upload/v123/scanpet/items/guid.ext
            var uri = new Uri(imageUrl);
            var path = uri.AbsolutePath;
            var uploadIndex = path.IndexOf("/upload/", StringComparison.Ordinal);
            if (uploadIndex < 0) return;

            var afterUpload = path[(uploadIndex + "/upload/".Length)..];
            // Remove version prefix (v123456789/)
            if (afterUpload.StartsWith('v') && afterUpload.Contains('/'))
            {
                afterUpload = afterUpload[(afterUpload.IndexOf('/') + 1)..];
            }
            // Remove extension
            var publicId = Path.ChangeExtension(afterUpload, null);

            _cloudinary.Destroy(new DeletionParams(publicId));
            _logger.LogInformation("Cloudinary image deleted: {PublicId}", publicId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete Cloudinary image: {Url}", imageUrl);
        }
    }
}

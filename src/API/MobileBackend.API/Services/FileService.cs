using MobileBackend.Application.Common.Interfaces;

namespace MobileBackend.API.Services;

/// <summary>
/// File service implementation for handling file uploads
/// Saves files to wwwroot/uploads directory
/// </summary>
public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder, CancellationToken cancellationToken = default)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", subfolder);
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/uploads/{subfolder}/{uniqueFileName}";
        _logger.LogInformation("File saved: {FilePath}", relativeUrl);
        return relativeUrl;
    }

    public void DeleteFile(string relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return;

        var filePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), relativeUrl.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("File deleted: {FilePath}", relativeUrl);
        }
    }
}

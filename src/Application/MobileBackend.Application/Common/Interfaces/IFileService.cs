namespace MobileBackend.Application.Common.Interfaces;

/// <summary>
/// Service interface for file upload operations
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Save a file and return the relative URL path
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="subfolder">Subfolder within uploads directory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relative URL path to the saved file</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file by its relative URL path
    /// </summary>
    /// <param name="relativeUrl">Relative URL path of the file</param>
    void DeleteFile(string relativeUrl);
}

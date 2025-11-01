namespace MobileBackend.Application.Common.Interfaces;

/// <summary>
/// Service for getting current date/time
/// Abstracted for testability (can mock in tests)
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Get current UTC date/time
    /// </summary>
    DateTime UtcNow { get; }
    
    /// <summary>
    /// Get current local date/time
    /// </summary>
    DateTime Now { get; }
}

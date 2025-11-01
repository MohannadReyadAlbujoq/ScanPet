using MobileBackend.Application.Common.Interfaces;

namespace MobileBackend.Infrastructure.Services;

/// <summary>
/// Implementation of DateTime service
/// Abstracted for testability (can be mocked in tests)
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
    
    public DateTime Now => DateTime.Now;
}

using System.Runtime.CompilerServices;
using NLog;

namespace MobileBackend.API.Logging;

/// <summary>
/// Helper service for structured logging with context
/// </summary>
public interface ILoggerService
{
    void LogTrace(string message, string? methodName = null, string? filePath = null, int lineNumber = 0);
    void LogInfo(string message, string? methodName = null, string? filePath = null, int lineNumber = 0);
    void LogWarning(string message, string? methodName = null, string? filePath = null, int lineNumber = 0);
    void LogError(string message, Exception? exception = null, string? methodName = null, string? filePath = null, int lineNumber = 0);
}

public class LoggerService : ILoggerService
{
    private readonly ILogger<LoggerService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggerService(
        ILogger<LoggerService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public void LogTrace(
        string message,
        [CallerMemberName] string? methodName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        Log(Microsoft.Extensions.Logging.LogLevel.Trace, message, null, methodName, filePath, lineNumber);
    }

    public void LogInfo(
        string message,
        [CallerMemberName] string? methodName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        Log(Microsoft.Extensions.Logging.LogLevel.Information, message, null, methodName, filePath, lineNumber);
    }

    public void LogWarning(
        string message,
        [CallerMemberName] string? methodName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        Log(Microsoft.Extensions.Logging.LogLevel.Warning, message, null, methodName, filePath, lineNumber);
    }

    public void LogError(
        string message,
        Exception? exception = null,
        [CallerMemberName] string? methodName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        var errorMessage = exception != null
            ? $"{message} - {exception.Message}{Environment.NewLine}{exception.StackTrace}"
            : message;

        Log(Microsoft.Extensions.Logging.LogLevel.Error, errorMessage, exception, methodName, filePath, lineNumber);
    }

    private void Log(
        Microsoft.Extensions.Logging.LogLevel level,
        string message,
        Exception? exception,
        string? methodName,
        string? filePath,
        int lineNumber)
    {
        var context = _httpContextAccessor.HttpContext;
        
        var logEvent = new LogEventInfo(
            NLog.LogLevel.FromString(level.ToString()),
            _logger.ToString(),
            message);

        if (context != null)
        {
            var activityId = System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString();
            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var requestType = context.Request.Method;
            var userName = context.User?.Identity?.Name ?? "Anonymous";

            logEvent.Properties["ActivityId"] = activityId;
            logEvent.Properties["Url"] = url;
            logEvent.Properties["RequestType"] = requestType;
            logEvent.Properties["UserName"] = userName;
        }

        logEvent.Properties["MethodName"] = methodName ?? "Unknown";
        logEvent.Properties["FilePath"] = filePath ?? "Unknown";
        logEvent.Properties["LineNumber"] = lineNumber;

        if (exception != null)
        {
            logEvent.Exception = exception;
        }

        NLog.LogManager.GetCurrentClassLogger().Log(logEvent);
    }
}

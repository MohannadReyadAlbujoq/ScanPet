using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using NLog;

namespace MobileBackend.API.Logging;

/// <summary>
/// Enhanced logging middleware that captures detailed request/response information
/// </summary>
public class EnhancedLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EnhancedLoggingMiddleware> _logger;

    public EnhancedLoggingMiddleware(
        RequestDelegate next,
        ILogger<EnhancedLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var activityId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        // Get user information
        var userName = context.User?.Identity?.Name ?? "Anonymous";
        
        // Build request URL
        var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        var requestType = context.Request.Method;

        // Log API Start
        LogWithContext(
            Microsoft.Extensions.Logging.LogLevel.Trace,
            "Api Start",
            activityId,
            url,
            requestType,
            userName,
            0,
            ".ctor",
            GetCallerFilePath(),
            GetCallerLineNumber()
        );

        // Capture original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware
            await _next(context);

            stopwatch.Stop();

            // Copy response back to original stream
            await responseBody.CopyToAsync(originalBodyStream);

            // Log completion
            LogRequestCompletion(
                activityId,
                url,
                requestType,
                userName,
                stopwatch.ElapsedMilliseconds
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log error
            LogWithContext(
                Microsoft.Extensions.Logging.LogLevel.Error,
                $"{ex.Message} - {ex.StackTrace}",
                activityId,
                url,
                requestType,
                userName,
                stopwatch.ElapsedMilliseconds,
                ex.TargetSite?.Name ?? "Unknown",
                GetExceptionFilePath(ex),
                GetExceptionLineNumber(ex)
            );

            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private void LogWithContext(
        Microsoft.Extensions.Logging.LogLevel level,
        string message,
        string activityId,
        string url,
        string requestType,
        string userName,
        long elapsed,
        string methodName,
        string filePath,
        int lineNumber)
    {
        var logEvent = new LogEventInfo(
            NLog.LogLevel.FromString(level.ToString()),
            _logger.ToString(),
            message);

        logEvent.Properties["ActivityId"] = activityId;
        logEvent.Properties["Url"] = url;
        logEvent.Properties["RequestType"] = requestType;
        logEvent.Properties["UserName"] = userName;
        logEvent.Properties["Elapsed"] = elapsed;
        logEvent.Properties["MethodName"] = methodName;
        logEvent.Properties["FilePath"] = filePath;
        logEvent.Properties["LineNumber"] = lineNumber;

        NLog.LogManager.GetCurrentClassLogger().Log(logEvent);
    }

    private void LogRequestCompletion(
        string activityId,
        string url,
        string requestType,
        string userName,
        long elapsed)
    {
        LogWithContext(
            Microsoft.Extensions.Logging.LogLevel.Information,
            $"Request completed in {elapsed}ms",
            activityId,
            url,
            requestType,
            userName,
            elapsed,
            "InvokeAsync",
            GetCallerFilePath(),
            GetCallerLineNumber()
        );
    }

    private string GetCallerFilePath([CallerFilePath] string filePath = "")
    {
        return filePath;
    }

    private int GetCallerLineNumber([CallerLineNumber] int lineNumber = 0)
    {
        return lineNumber;
    }

    private string GetExceptionFilePath(Exception ex)
    {
        var stackTrace = new StackTrace(ex, true);
        var frame = stackTrace.GetFrame(0);
        return frame?.GetFileName() ?? "Unknown";
    }

    private int GetExceptionLineNumber(Exception ex)
    {
        var stackTrace = new StackTrace(ex, true);
        var frame = stackTrace.GetFrame(0);
        return frame?.GetFileLineNumber() ?? 0;
    }
}

/// <summary>
/// Extension method to register enhanced logging middleware
/// </summary>
public static class EnhancedLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseEnhancedLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<EnhancedLoggingMiddleware>();
    }
}

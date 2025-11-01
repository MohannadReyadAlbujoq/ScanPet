using System.Diagnostics;
using System.Text;

namespace MobileBackend.API.Middleware;

/// <summary>
/// Logs HTTP requests and responses for debugging and monitoring
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        await LogRequest(context);

        // Capture original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware
            await _next(context);

            stopwatch.Stop();

            // Log response
            await LogResponse(context, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();

        var request = context.Request;
        var requestBody = string.Empty;

        if (request.ContentLength > 0)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);

            // Mask sensitive data
            if (requestBody.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                requestBody = "***SENSITIVE DATA MASKED***";
            }
        }

        _logger.LogInformation(
            "HTTP Request: {Method} {Path} | QueryString: {QueryString} | Body: {Body}",
            request.Method,
            request.Path,
            request.QueryString,
            requestBody
        );
    }

    private async Task LogResponse(HttpContext context, long elapsedMilliseconds)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        var statusCode = context.Response.StatusCode;
        var logLevel = statusCode >= 500 ? LogLevel.Error :
                      statusCode >= 400 ? LogLevel.Warning :
                      LogLevel.Information;

        _logger.Log(
            logLevel,
            "HTTP Response: {StatusCode} | Duration: {Duration}ms | Body: {Body}",
            statusCode,
            elapsedMilliseconds,
            responseBody.Length > 1000 ? $"{responseBody.Substring(0, 1000)}..." : responseBody
        );
    }
}

/// <summary>
/// Extension method to register request/response logging middleware
/// </summary>
public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}

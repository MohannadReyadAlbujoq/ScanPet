using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Interfaces;
using System.Security.Claims;

namespace MobileBackend.API.Middleware;

/// <summary>
/// Middleware for automatic audit logging of HTTP requests
/// Logs user actions, IP addresses, and request metadata
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    // HTTP methods that should be audited
    private static readonly HashSet<string> AuditedMethods = new(StringComparer.OrdinalIgnoreCase) 
    { 
        "POST", "PUT", "DELETE", "PATCH" 
    };

    // Paths that should be excluded from audit logging
    private static readonly string[] ExcludedPaths = 
    { 
        "/health", 
        "/swagger", 
        "/api/auth/refresh", // Too frequent
        "/api/auth/me"       // Read-only
    };

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        // Only audit specific HTTP methods
        if (!AuditedMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Exclude certain paths
        var path = context.Request.Path.Value ?? string.Empty;
        if (ExcludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Extract user information
        var user = context.User;
        Guid? userId = null;
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }
        }

        // Extract request metadata
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        // Determine action and entity from request
        var action = GetActionFromRequest(context);
        var (entityName, entityId) = ExtractEntityInfo(context);

        try
        {
            // Call next middleware
            await _next(context);

            // Log successful audit (only for 2xx responses)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                if (userId.HasValue)
                {
                    await auditService.LogAsync(
                        action,
                        entityName,
                        entityId ?? Guid.Empty,
                        userId.Value,
                        $"IP: {ipAddress}, UserAgent: {userAgent}"
                    );

                    // Persist audit log outside of handler transaction
                    await unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(
                        "Audit: User {UserId} performed {Action} on {EntityName} {EntityId} from {IpAddress}",
                        userId.Value, action, entityName, entityId, ipAddress
                    );
                }
            }
        }
        catch (Exception ex)
        {
            // Log failed audit
            if (userId.HasValue)
            {
                try
                {
                    await auditService.LogAsync(
                        $"{action}_FAILED",
                        entityName,
                        entityId ?? Guid.Empty,
                        userId.Value,
                        $"Error: {ex.Message}, IP: {ipAddress}, UserAgent: {userAgent}"
                    );

                    await unitOfWork.SaveChangesAsync();
                }
                catch
                {
                    // Don't let audit logging failure mask the original exception
                }
            }

            _logger.LogWarning(
                ex,
                "Audit (Failed): User {UserId} attempted {Action} on {EntityName} {EntityId} - Error: {Error}",
                userId, action, entityName, entityId, ex.Message
            );

            throw; // Re-throw to be handled by exception middleware
        }
    }

    private static string GetActionFromRequest(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;

        return method switch
        {
            "POST" when path.Contains("/login") => "Login",
            "POST" when path.Contains("/logout") => "Logout",
            "POST" when path.Contains("/register") => "Register",
            "POST" when path.Contains("/confirm") => "Confirm",
            "POST" when path.Contains("/cancel") => "Cancel",
            "POST" when path.Contains("/approve") => "Approve",
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Patch",
            "DELETE" => "Delete",
            _ => "Unknown"
        };
    }

    private static (string EntityName, Guid? EntityId) ExtractEntityInfo(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Expected format: /api/{controller}/{id?}
        string entityName = "Unknown";
        Guid? entityId = null;

        if (segments.Length >= 2)
        {
            entityName = segments[1]; // e.g., "users", "orders", "items"

            // Try to parse ID from path (usually the 3rd segment)
            if (segments.Length >= 3 && Guid.TryParse(segments[2], out var parsedId))
            {
                entityId = parsedId;
            }
        }

        return (entityName, entityId);
    }
}

/// <summary>
/// Extension method to register audit logging middleware
/// </summary>
public static class AuditLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditLoggingMiddleware>();
    }
}

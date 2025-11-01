using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace MobileBackend.API.Middleware;

/// <summary>
/// JWT middleware for custom token validation and user context setup
/// Extracts user information from JWT claims and makes it available via CurrentUserService
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Token is already validated by JWT Bearer authentication middleware
                // This middleware just logs and enhances the context

                var user = context.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var username = user.FindFirst(ClaimTypes.Name)?.Value;
                    var email = user.FindFirst(ClaimTypes.Email)?.Value;
                    var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                    _logger.LogDebug(
                        "Authenticated user: UserId={UserId}, Username={Username}, Email={Email}, Roles={Roles}",
                        userId, username, email, string.Join(", ", roles)
                    );

                    // Add custom claims to HttpContext.Items for easy access
                    context.Items["UserId"] = userId;
                    context.Items["Username"] = username;
                    context.Items["Email"] = email;
                    context.Items["Roles"] = roles;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process JWT token");
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register JWT middleware
/// </summary>
public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtMiddleware>();
    }
}

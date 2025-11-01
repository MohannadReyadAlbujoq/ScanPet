using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MobileBackend.Domain.Enums;
using MobileBackend.Application.Interfaces;
using System.Security.Claims;

namespace MobileBackend.API.Filters;

/// <summary>
/// Authorization filter that checks if the current user has specific permissions
/// Usage: [RequirePermission(PermissionType.UserManagement_Create)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(PermissionType permission) 
        : base(typeof(RequirePermissionFilter))
    {
        Arguments = new object[] { permission };
    }
}

/// <summary>
/// Filter implementation for permission checking
/// </summary>
public class RequirePermissionFilter : IAsyncAuthorizationFilter
{
    private readonly PermissionType _requiredPermission;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILogger<RequirePermissionFilter> _logger;

    public RequirePermissionFilter(
        PermissionType requiredPermission,
        IPermissionRepository permissionRepository,
        ILogger<RequirePermissionFilter> logger)
    {
        _requiredPermission = requiredPermission;
        _permissionRepository = permissionRepository;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = "You must be authenticated to access this resource."
            });
            return;
        }

        // Get user ID from claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("User ID claim not found or invalid");
            context.Result = new ForbidResult();
            return;
        }

        // Check if user has the required permission
        try
        {
            var hasPermission = await _permissionRepository.HasPermissionAsync(userId, _requiredPermission);
            
            if (!hasPermission)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access {Action} without {Permission} permission",
                    userId,
                    context.ActionDescriptor.DisplayName,
                    _requiredPermission
                );

                context.Result = new ObjectResult(new
                {
                    success = false,
                    message = $"You do not have permission to perform this action. Required permission: {_requiredPermission}"
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            _logger.LogDebug(
                "User {UserId} has {Permission} permission for {Action}",
                userId,
                _requiredPermission,
                context.ActionDescriptor.DisplayName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", _requiredPermission, userId);
            
            context.Result = new ObjectResult(new
            {
                success = false,
                message = "An error occurred while checking permissions."
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}

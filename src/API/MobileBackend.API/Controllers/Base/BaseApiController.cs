using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.API.Controllers.Base;

/// <summary>
/// Base controller with shared functionality for all API controllers
/// Provides common response formatting, error handling, and utilities
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected readonly ILogger Logger;

    protected BaseApiController(IMediator mediator, ILogger logger)
    {
        Mediator = mediator;
        Logger = logger;
    }

    #region Success Responses

    /// <summary>
    /// Returns a standardized success response with data
    /// </summary>
    protected IActionResult OkResponse<T>(T data, string? message = null)
    {
        return Ok(new
        {
            success = true,
            message = message ?? "Operation successful",
            data
        });
    }

    /// <summary>
    /// Returns a standardized success response without data
    /// </summary>
    protected IActionResult OkResponse(string message = "Operation successful")
    {
        return Ok(new
        {
            success = true,
            message
        });
    }

    /// <summary>
    /// Returns a standardized created response
    /// </summary>
    protected IActionResult CreatedResponse<T>(T data, string message = "Resource created successfully")
    {
        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message,
            data
        });
    }

    /// <summary>
    /// Returns a standardized created response with ID
    /// </summary>
    protected IActionResult CreatedResponse(Guid id, string entityName)
    {
        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = $"{entityName} created successfully",
            id
        });
    }

    #endregion

    #region Error Responses

    /// <summary>
    /// Returns a standardized error response from Result object
    /// </summary>
    protected IActionResult ErrorResponse<T>(Result<T> result)
    {
        return StatusCode(result.StatusCode, new
        {
            success = false,
            message = result.ErrorMessage,
            errors = result.ValidationErrors
        });
    }

    /// <summary>
    /// Returns a standardized bad request response
    /// </summary>
    protected IActionResult BadRequestResponse(string message, object? errors = null)
    {
        return BadRequest(new
        {
            success = false,
            message,
            errors
        });
    }

    /// <summary>
    /// Returns a standardized not found response
    /// </summary>
    protected IActionResult NotFoundResponse(string entityName)
    {
        return NotFound(new
        {
            success = false,
            message = $"{entityName} not found"
        });
    }

    /// <summary>
    /// Returns a standardized unauthorized response
    /// </summary>
    protected IActionResult UnauthorizedResponse(string message = "Unauthorized access")
    {
        return Unauthorized(new
        {
            success = false,
            message
        });
    }

    /// <summary>
    /// Returns a standardized forbidden response
    /// </summary>
    protected IActionResult ForbiddenResponse(string message = "Access forbidden")
    {
        return StatusCode(StatusCodes.Status403Forbidden, new
        {
            success = false,
            message
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Validates ID mismatch between route and body
    /// </summary>
    protected bool ValidateIdMismatch(Guid routeId, Guid bodyId, out IActionResult? errorResponse)
    {
        if (routeId != bodyId)
        {
            errorResponse = BadRequestResponse("ID mismatch between route and request body");
            return false;
        }
        errorResponse = null;
        return true;
    }

    /// <summary>
    /// Handles Result object and returns appropriate response
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result, string? successMessage = null)
    {
        if (!result.Success)
        {
            return ErrorResponse(result);
        }

        return successMessage != null
            ? OkResponse(result.Data, successMessage)
            : OkResponse(result.Data);
    }

    /// <summary>
    /// Handles non-generic Result and returns appropriate response
    /// </summary>
    protected IActionResult HandleResult(Result result, string? successMessage = null)
    {
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage,
                errors = result.ValidationErrors
            });
        }

        return successMessage != null
            ? OkResponse(successMessage)
            : OkResponse();
    }

    /// <summary>
    /// Handles boolean Result and returns appropriate response
    /// </summary>
    protected IActionResult HandleBoolResult(Result<bool> result, string successMessage)
    {
        if (!result.Success)
        {
            return ErrorResponse(result);
        }

        return OkResponse(successMessage);
    }

    /// <summary>
    /// Handles Guid Result for create operations
    /// </summary>
    protected IActionResult HandleCreateResult(Result<Guid> result, string entityName)
    {
        if (!result.Success)
        {
            return ErrorResponse(result);
        }

        return CreatedResponse(result.Data, entityName);
    }

    /// <summary>
    /// Gets current user ID from claims
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets current username from claims
    /// </summary>
    protected string? GetCurrentUsername()
    {
        return User.FindFirst("name")?.Value;
    }

    /// <summary>
    /// Gets client IP address
    /// </summary>
    protected string? GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Logs operation with standard format
    /// </summary>
    protected void LogOperation(string operation, string entityName, Guid? entityId = null)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        if (entityId.HasValue)
        {
            Logger.LogInformation(
                "{Operation} {EntityName} - EntityId: {EntityId}, User: {Username} ({UserId})",
                operation, entityName, entityId, username, userId);
        }
        else
        {
            Logger.LogInformation(
                "{Operation} {EntityName} - User: {Username} ({UserId})",
                operation, entityName, username, userId);
        }
    }

    /// <summary>
    /// Logs error with standard format
    /// </summary>
    protected void LogError(string operation, string entityName, Exception ex, Guid? entityId = null)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        if (entityId.HasValue)
        {
            Logger.LogError(ex,
                "Error in {Operation} {EntityName} - EntityId: {EntityId}, User: {Username} ({UserId})",
                operation, entityName, entityId, username, userId);
        }
        else
        {
            Logger.LogError(ex,
                "Error in {Operation} {EntityName} - User: {Username} ({UserId})",
                operation, entityName, username, userId);
        }
    }

    #endregion
}

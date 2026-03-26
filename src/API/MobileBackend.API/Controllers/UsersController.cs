using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
using MobileBackend.Application.DTOs.Users;
using MobileBackend.Application.Features.Users.Commands.ApproveUser;
using MobileBackend.Application.Features.Users.Commands.CreateUser;
using MobileBackend.Application.Features.Users.Commands.ToggleUserStatus;
using MobileBackend.Application.Features.Users.Commands.UpdateUserRole;
using MobileBackend.Application.Features.Users.Queries.GetAllUsers;
using MobileBackend.Application.Features.Users.Queries.GetUserById;

namespace MobileBackend.API.Controllers;

/// <summary>
/// User management controller
/// Admin-only operations for managing user accounts
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseApiController
{
    public UsersController(IMediator mediator, ILogger<UsersController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserByIdQuery { UserId = id };
        var result = await Mediator.Send(query);

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    /// <param name="dto">User creation data</param>
    /// <returns>Created user ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] UserDto dto)
    {
        // Validate required fields explicitly
        if (string.IsNullOrWhiteSpace(dto.Username) || 
            string.IsNullOrWhiteSpace(dto.Email) || 
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequestResponse("Username, Email, and Password are required");
        }

        var command = new CreateUserCommand
        {
            Username = dto.Username,
            Email = dto.Email,
            Password = dto.Password,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? CreatedResponse(result.Data, "User") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Activate a user account (approve + enable).
    /// Used by admin to activate newly registered users.
    /// No request body needed — just provide user ID in the URL.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var command = new ApproveUserCommand
        {
            UserId = id,
            IsApproved = true,
            IsEnabled = true
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("User activated successfully") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Deactivate a user account (revoke approval + disable).
    /// Used by admin to fully deactivate a user.
    /// No request body needed — just provide user ID in the URL.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var command = new ApproveUserCommand
        {
            UserId = id,
            IsApproved = false,
            IsEnabled = false
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("User deactivated successfully") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Enable a user account (keep approved but re-enable).
    /// Used by admin to re-enable a previously disabled user.
    /// No request body needed — just provide user ID in the URL.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Enable(Guid id)
    {
        var command = new ToggleUserStatusCommand
        {
            UserId = id,
            IsEnabled = true
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("User enabled successfully") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Disable a user account (keep approved but disable login).
    /// Used by admin to temporarily disable a user without revoking approval.
    /// No request body needed — just provide user ID in the URL.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Disable(Guid id)
    {
        var command = new ToggleUserStatusCommand
        {
            UserId = id,
            IsEnabled = false
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("User disabled successfully") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Update user's role
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Role update details</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleDto request)
    {
        var command = new UpdateUserRoleCommand
        {
            UserId = id,
            RoleId = request.RoleId
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("User role updated successfully") 
            : ErrorResponse(result);
    }
}

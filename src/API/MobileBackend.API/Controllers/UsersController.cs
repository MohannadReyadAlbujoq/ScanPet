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
    /// Approve or reject user account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Approval details</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] UserApprovalDto request)
    {
        if (id != request.UserId)
        {
            return BadRequestResponse("User ID mismatch");
        }

        var command = new ApproveUserCommand
        {
            UserId = request.UserId,
            IsApproved = request.IsApproved,
            IsEnabled = request.IsEnabled
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse($"User {(request.IsApproved ? "approved" : "rejected")} successfully") 
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

    /// <summary>
    /// Enable or disable user account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Status update details</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] ToggleUserStatusDto request)
    {
        var command = new ToggleUserStatusCommand
        {
            UserId = id,
            IsEnabled = request.IsEnabled
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse($"User {(request.IsEnabled ? "enabled" : "disabled")} successfully") 
            : ErrorResponse(result);
    }
}

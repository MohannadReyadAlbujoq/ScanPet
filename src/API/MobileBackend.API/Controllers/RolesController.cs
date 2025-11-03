using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
using MobileBackend.Application.DTOs.Roles;
using MobileBackend.Application.Features.Roles.Commands.AssignPermissions;
using MobileBackend.Application.Features.Roles.Commands.CreateRole;
using MobileBackend.Application.Features.Roles.Commands.DeleteRole;
using MobileBackend.Application.Features.Roles.Commands.UpdateRole;
using MobileBackend.Application.Features.Roles.Queries.GetAllRoles;
using MobileBackend.Application.Features.Roles.Queries.GetRoleById;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Role management controller
/// Handles role CRUD and permission assignment
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RolesController : BaseApiController
{
    public RolesController(IMediator mediator, ILogger<RolesController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    /// <returns>List of all roles with permissions</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllRolesQuery();
        var result = await Mediator.Send(query);

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role details with permissions</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetRoleByIdQuery { RoleId = id };
        var result = await Mediator.Send(query);

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <param name="dto">Role creation data</param>
    /// <returns>Created role ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] RoleDto dto)
    {
        var command = new CreateRoleCommand
        {
            Name = dto.Name ?? string.Empty,
            Description = dto.Description
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? CreatedResponse(result.Data, "Role") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="dto">Updated role data</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoleDto dto)
    {
        var command = new UpdateRoleCommand
        {
            RoleId = id,
            Name = dto.Name ?? string.Empty,
            Description = dto.Description
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("Role updated successfully") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Delete a role (soft delete)
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteRoleCommand { RoleId = id };
        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("Role deleted successfully") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Assign permissions to a role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="request">Permissions to assign</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsDto request)
    {
        if (id != request.RoleId)
        {
            return BadRequestResponse("Role ID mismatch");
        }

        var command = new AssignPermissionsCommand
        {
            RoleId = request.RoleId,
            Permissions = request.Permissions
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("Permissions assigned successfully") 
            : ErrorResponse(result);
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.Features.Colors.Commands.CreateColor;
using MobileBackend.Application.Features.Colors.Commands.DeleteColor;
using MobileBackend.Application.Features.Colors.Commands.UpdateColor;
using MobileBackend.Application.Features.Colors.Queries.GetAllColors;
using MobileBackend.Application.Features.Colors.Queries.GetColorById;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Color management controller
/// Handles color CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ColorsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ColorsController> _logger;

    public ColorsController(IMediator mediator, ILogger<ColorsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all colors
    /// </summary>
    /// <returns>List of all colors</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllColorsQuery();
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Get color by ID
    /// </summary>
    /// <param name="id">Color ID</param>
    /// <returns>Color details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetColorByIdQuery { ColorId = id };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Create a new color
    /// </summary>
    /// <param name="dto">Color creation data</param>
    /// <returns>Created color ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] ColorDto dto)
    {
        var command = new CreateColorCommand
        {
            Name = dto.Name ?? string.Empty,
            Description = dto.Description,
            RedValue = dto.RedValue ?? 0,
            GreenValue = dto.GreenValue ?? 0,
            BlueValue = dto.BlueValue ?? 0
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage,
                errors = result.ValidationErrors
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = "Color created successfully",
            colorId = result.Data
        });
    }

    /// <summary>
    /// Update an existing color
    /// </summary>
    /// <param name="id">Color ID</param>
    /// <param name="dto">Updated color data</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ColorDto dto)
    {
        var command = new UpdateColorCommand
        {
            ColorId = id,
            Name = dto.Name ?? string.Empty,
            Description = dto.Description,
            RedValue = dto.RedValue ?? 0,
            GreenValue = dto.GreenValue ?? 0,
            BlueValue = dto.BlueValue ?? 0
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            success = true,
            message = "Color updated successfully"
        });
    }

    /// <summary>
    /// Delete a color (soft delete)
    /// </summary>
    /// <param name="id">Color ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteColorCommand { ColorId = id };
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            success = true,
            message = "Color deleted successfully"
        });
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.Features.Colors.Commands.CreateColor;
using MobileBackend.Application.Features.Colors.Commands.DeleteColor;
using MobileBackend.Application.Features.Colors.Commands.UpdateColor;
using MobileBackend.Application.Features.Colors.Queries.GetAllColors;
using MobileBackend.Application.Features.Colors.Queries.GetColorById;
using MobileBackend.Application.Features.Colors.Queries.SearchColors;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Color management controller
/// Handles color CRUD operations and search
/// </summary>
[Route("api/[controller]")]
public class ColorsController : BaseApiController
{
    public ColorsController(IMediator mediator, ILogger<ColorsController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Get all colors
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllColorsQuery();
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Search colors by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="pageNumber">Page number (optional)</param>
    /// <param name="pageSize">Page size (optional)</param>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] string searchTerm,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequestResponse("Search term is required");
        }

        var query = new SearchColorsQuery
        {
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);
        LogOperation("Search Colors", "Color");
        return HandleResult(result);
    }

    /// <summary>
    /// Get color by ID
    /// </summary>
    /// <param name="id">Color ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetColorByIdQuery { ColorId = id };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new color
    /// </summary>
    /// <param name="dto">Color creation data</param>
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

        var result = await Mediator.Send(command);
        
        if (result.Success)
        {
            LogOperation("Create", "Color", result.Data);
        }

        return HandleCreateResult(result, "Color");
    }

    /// <summary>
    /// Update an existing color
    /// </summary>
    /// <param name="id">Color ID</param>
    /// <param name="dto">Updated color data</param>
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

        var result = await Mediator.Send(command);
        
        if (result.Success)
        {
            LogOperation("Update", "Color", id);
        }

        return HandleBoolResult(result, "Color updated successfully");
    }

    /// <summary>
    /// Delete a color (soft delete)
    /// </summary>
    /// <param name="id">Color ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteColorCommand { ColorId = id };
        var result = await Mediator.Send(command);
        
        if (result.Success)
        {
            LogOperation("Delete", "Color", id);
        }

        return HandleBoolResult(result, "Color deleted successfully");
    }
}

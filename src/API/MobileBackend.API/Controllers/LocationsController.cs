using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.Application.DTOs.Locations;
using MobileBackend.Application.Features.Locations.Commands.CreateLocation;
using MobileBackend.Application.Features.Locations.Commands.DeleteLocation;
using MobileBackend.Application.Features.Locations.Commands.UpdateLocation;
using MobileBackend.Application.Features.Locations.Queries.GetAllLocations;
using MobileBackend.Application.Features.Locations.Queries.GetLocationById;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Location management controller
/// Handles location CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(IMediator mediator, ILogger<LocationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all locations
    /// </summary>
    /// <returns>List of all locations</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllLocationsQuery();
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
    /// Get location by ID
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <returns>Location details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetLocationByIdQuery { LocationId = id };
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
    /// Create a new location
    /// </summary>
    /// <param name="dto">Location creation data</param>
    /// <returns>Created location ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] LocationDto dto)
    {
        var command = new CreateLocationCommand
        {
            Name = dto.Name ?? string.Empty,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            IsActive = dto.IsActive ?? true
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
            message = "Location created successfully",
            locationId = result.Data
        });
    }

    /// <summary>
    /// Update an existing location
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <param name="dto">Updated location data</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] LocationDto dto)
    {
        var command = new UpdateLocationCommand
        {
            LocationId = id,
            Name = dto.Name ?? string.Empty,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            IsActive = dto.IsActive ?? true
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
            message = "Location updated successfully"
        });
    }

    /// <summary>
    /// Delete a location (soft delete)
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteLocationCommand { LocationId = id };
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
            message = "Location deleted successfully"
        });
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.Application.DTOs.Items;
using MobileBackend.Application.Features.Items.Commands.CreateItem;
using MobileBackend.Application.Features.Items.Commands.DeleteItem;
using MobileBackend.Application.Features.Items.Commands.UpdateItem;
using MobileBackend.Application.Features.Items.Queries.GetAllItems;
using MobileBackend.Application.Features.Items.Queries.GetItemById;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Item management controller
/// Handles item CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IMediator mediator, ILogger<ItemsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all items
    /// </summary>
    /// <param name="pageNumber">Page number (optional)</param>
    /// <param name="pageSize">Page size (optional)</param>
    /// <returns>List of all items</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        var query = new GetAllItemsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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
    /// Get item by ID
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Item details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetItemByIdQuery { ItemId = id };
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
    /// Create a new item
    /// </summary>
    /// <param name="dto">Item creation data</param>
    /// <returns>Created item ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] ItemDto dto)
    {
        var command = new CreateItemCommand
        {
            Name = dto.Name ?? string.Empty,
            Description = dto.Description,
            SKU = dto.SKU,
            BasePrice = dto.BasePrice ?? 0,
            Quantity = dto.Quantity ?? 0,
            ColorId = dto.ColorId,
            ImageUrl = dto.ImageUrl
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
            message = "Item created successfully",
            itemId = result.Data
        });
    }

    /// <summary>
    /// Update an existing item
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="dto">Updated item data</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ItemDto dto)
    {
        var command = new UpdateItemCommand
        {
            ItemId = id,
            Name = dto.Name ?? string.Empty,
            Description = dto.Description,
            SKU = dto.SKU,
            BasePrice = dto.BasePrice ?? 0,
            Quantity = dto.Quantity ?? 0,
            ColorId = dto.ColorId,
            ImageUrl = dto.ImageUrl
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
            message = "Item updated successfully"
        });
    }

    /// <summary>
    /// Delete an item (soft delete)
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteItemCommand { ItemId = id };
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
            message = "Item deleted successfully"
        });
    }
}

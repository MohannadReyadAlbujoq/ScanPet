using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
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
public class ItemsController : BaseApiController
{
    public ItemsController(IMediator mediator, ILogger<ItemsController> logger)
        : base(mediator, logger)
    {
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
        var result = await Mediator.Send(query);

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
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
        var result = await Mediator.Send(query);

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
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

        var result = await Mediator.Send(command);

        return result.Success 
            ? CreatedResponse(result.Data, "Item") 
            : ErrorResponse(result);
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

        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("Item updated successfully") 
            : ErrorResponse(result);
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
        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("Item deleted successfully") 
            : ErrorResponse(result);
    }
}

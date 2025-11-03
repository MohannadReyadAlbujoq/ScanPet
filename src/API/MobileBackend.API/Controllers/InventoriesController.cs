using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Features.Inventories.Commands.AdjustInventory;
using MobileBackend.Application.Features.Inventories.Commands.CreateInventory;
using MobileBackend.Application.Features.Inventories.Commands.DeleteInventory;
using MobileBackend.Application.Features.Inventories.Commands.SetItemInventory;
using MobileBackend.Application.Features.Inventories.Commands.TransferInventory;
using MobileBackend.Application.Features.Inventories.Commands.UpdateInventory;
using MobileBackend.Application.Features.Inventories.Queries.GetActiveInventories;
using MobileBackend.Application.Features.Inventories.Queries.GetAllInventories;
using MobileBackend.Application.Features.Inventories.Queries.GetInventoryById;
using MobileBackend.Application.Features.Inventories.Queries.GetItemInventory;
using MobileBackend.Application.Features.Inventories.Queries.GetItemsInInventory;
using MobileBackend.Application.Features.Inventories.Queries.GetLowStockItems;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Inventory/Warehouse Management Controller
/// Manages warehouse locations and item storage
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoriesController : BaseApiController
{
    public InventoriesController(IMediator mediator, ILogger<InventoriesController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Create a new inventory/warehouse
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryDto dto)
    {
        var command = new CreateInventoryCommand
        {
            Name = dto.Name,
            Location = dto.Location,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        var result = await Mediator.Send(command);
        return HandleCreateResult(result, "Inventory");
    }

    /// <summary>
    /// Get all inventories/warehouses
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllInventories()
    {
        var query = new GetAllInventoriesQuery();
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get inventory by ID with items
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInventoryById(Guid id)
    {
        var query = new GetInventoryByIdQuery { Id = id };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Update an inventory
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInventory(Guid id, [FromBody] UpdateInventoryDto dto)
    {
        var command = new UpdateInventoryCommand
        {
            Id = id,
            Name = dto.Name,
            Location = dto.Location,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        var result = await Mediator.Send(command);
        return HandleBoolResult(result, "Inventory updated successfully");
    }

    /// <summary>
    /// Delete an inventory (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInventory(Guid id)
    {
        var command = new DeleteInventoryCommand { Id = id };
        var result = await Mediator.Send(command);
        return HandleBoolResult(result, "Inventory deleted successfully");
    }

    /// <summary>
    /// Get all items in a specific inventory
    /// </summary>
    [HttpGet("{inventoryId}/items")]
    public async Task<IActionResult> GetItemsInInventory(Guid inventoryId)
    {
        var query = new GetItemsInInventoryQuery { InventoryId = inventoryId };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get item inventory across all warehouses
    /// </summary>
    [HttpGet("items/{itemId}")]
    public async Task<IActionResult> GetItemInventory(Guid itemId)
    {
        var query = new GetItemInventoryQuery { ItemId = itemId };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Set or update item inventory at a warehouse
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> SetItemInventory([FromBody] SetItemInventoryDto dto)
    {
        var command = new SetItemInventoryCommand
        {
            ItemId = dto.ItemId,
            InventoryId = dto.InventoryId,
            Quantity = dto.Quantity,
            MinimumQuantity = dto.MinimumQuantity,
            MaximumQuantity = dto.MaximumQuantity,
            Notes = dto.Notes
        };

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Adjust inventory quantity (add or remove stock)
    /// </summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustInventory([FromBody] AdjustInventoryDto dto)
    {
        var command = new AdjustInventoryCommand
        {
            ItemId = dto.ItemId,
            InventoryId = dto.InventoryId,
            QuantityChange = dto.QuantityChange,
            Notes = dto.Notes,
            Reason = dto.Reason
        };

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Transfer inventory between warehouses
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> TransferInventory([FromBody] TransferInventoryDto dto)
    {
        var command = new TransferInventoryCommand
        {
            ItemId = dto.ItemId,
            FromInventoryId = dto.FromInventoryId,
            ToInventoryId = dto.ToInventoryId,
            Quantity = dto.Quantity,
            Notes = dto.Notes,
            Reason = dto.Reason
        };

        var result = await Mediator.Send(command);
        return HandleBoolResult(result, "Inventory transferred successfully");
    }

    /// <summary>
    /// Get low stock items across all inventories
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockItems()
    {
        var query = new GetLowStockItemsQuery();
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get active inventories only
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveInventories()
    {
        var query = new GetActiveInventoriesQuery();
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}

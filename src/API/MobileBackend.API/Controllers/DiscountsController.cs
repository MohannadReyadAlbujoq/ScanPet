using MediatR;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
using MobileBackend.Application.DTOs.Discounts;
using MobileBackend.Application.Features.Discounts.Commands.DeleteDiscount;
using MobileBackend.Application.Features.Discounts.Commands.UpsertDiscount;
using MobileBackend.Application.Features.Discounts.Queries.GetDiscountsForItem;

namespace MobileBackend.API.Controllers;

/// <summary>
/// v5 Discount management. Discounts attach to an Item (Scope=0), an
/// ItemInventory pair (Scope=1) or every ItemInventory in a Location (Scope=2).
/// Stack/exclusive behaviour is controlled per-row via IsStackable.
/// </summary>
[Route("api/[controller]")]
public class DiscountsController : BaseApiController
{
    public DiscountsController(IMediator mediator, ILogger<DiscountsController> logger)
        : base(mediator, logger) { }

    /// <summary>List all discounts attached to a given item (Item / ItemInventory / ItemLocation).</summary>
    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetForItem(Guid itemId)
    {
        var result = await Mediator.Send(new GetDiscountsForItemQuery { ItemId = itemId });
        return result.Success ? OkResponse(result.Data) : ErrorResponse(result);
    }

    /// <summary>Create a new discount.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertDiscountRequest body)
    {
        var cmd = new UpsertDiscountCommand
        {
            Scope = body.Scope,
            ItemId = body.ItemId,
            InventoryId = body.InventoryId,
            LocationId = body.LocationId,
            Amount = body.Amount,
            Label = body.Label,
            IsStackable = body.IsStackable,
            IsRevertable = body.IsRevertable,
            StartsAt = body.StartsAt,
            ExpiresAt = body.ExpiresAt
        };
        var result = await Mediator.Send(cmd);
        return result.Success ? CreatedResponse(result.Data, "Discount") : ErrorResponse(result);
    }

    /// <summary>Update an existing discount.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertDiscountRequest body)
    {
        var cmd = new UpsertDiscountCommand
        {
            Id = id,
            Scope = body.Scope,
            ItemId = body.ItemId,
            InventoryId = body.InventoryId,
            LocationId = body.LocationId,
            Amount = body.Amount,
            Label = body.Label,
            IsStackable = body.IsStackable,
            IsRevertable = body.IsRevertable,
            StartsAt = body.StartsAt,
            ExpiresAt = body.ExpiresAt
        };
        var result = await Mediator.Send(cmd);
        return result.Success ? OkResponse(result.Data) : ErrorResponse(result);
    }

    /// <summary>Soft-delete a discount.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteDiscountCommand { Id = id });
        return result.Success ? OkResponse("Discount deleted") : ErrorResponse(result);
    }
}

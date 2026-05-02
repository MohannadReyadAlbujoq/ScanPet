using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
using MobileBackend.Application.Common.Interfaces;
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
[Route("api/[controller]")]
public class ItemsController : BaseApiController
{
    private readonly IFileService _fileService;

    public ItemsController(IMediator mediator, ILogger<ItemsController> logger, IFileService fileService)
        : base(mediator, logger)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Get all items with pagination and optional filtering
    /// </summary>
    /// <param name="pageNumber">Page number (defaults to 1)</param>
    /// <param name="pageSize">Page size (defaults to 10, max 100)</param>
    /// <param name="inventoryId">Optional inventory/section ID to filter items by</param>
    /// <returns>Paged list of items with pagination metadata</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? inventoryId = null,
        [FromQuery] string? keyword = null)
    {
        var query = new GetAllItemsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            InventoryId = inventoryId,
            Keyword = keyword
        };
        var result = await Mediator.Send(query);

        if (result.Success && result.Data?.Items != null)
        {
            ResolveImageUrls(result.Data.Items);
        }

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

        if (result.Success && result.Data != null)
        {
            ResolveImageUrl(result.Data);
        }

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Create a new item (supports file upload via multipart/form-data)
    /// </summary>
    /// <param name="name">Item name</param>
    /// <param name="description">Item description</param>
    /// <param name="sku">Stock Keeping Unit</param>
    /// <param name="basePrice">Base price</param>
    /// <param name="colorId">Color ID</param>
    /// <param name="image">Image file</param>
    /// <returns>Created item ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] string name,
        [FromForm] string? description,
        [FromForm] string? sku,
        [FromForm] decimal basePrice,
        [FromForm] Guid? colorId,
        IFormFile? image)
    {
        string? imageUrl = null;
        if (image != null && image.Length > 0)
        {
            using var stream = image.OpenReadStream();
            imageUrl = await _fileService.SaveFileAsync(stream, image.FileName, "items");
        }

        var command = new CreateItemCommand
        {
            Name = name,
            Description = description,
            SKU = sku,
            BasePrice = basePrice,
            ColorId = colorId,
            ImageUrl = imageUrl
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? CreatedResponse(result.Data, "Item") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Update an existing item (supports file upload via multipart/form-data)
    /// If no new image is uploaded, the existing image is preserved.
    /// If a new image is uploaded, the old file is NOT deleted (preserved).
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="name">Item name</param>
    /// <param name="description">Item description</param>
    /// <param name="sku">Stock Keeping Unit</param>
    /// <param name="basePrice">Base price</param>
    /// <param name="colorId">Color ID</param>
    /// <param name="imageUrl">Existing image URL (preserved if no new file)</param>
    /// <param name="image">New image file (optional)</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromForm] string name,
        [FromForm] string? description,
        [FromForm] string? sku,
        [FromForm] decimal basePrice,
        [FromForm] Guid? colorId,
        [FromForm] string? imageUrl,
        IFormFile? image)
    {
        string? finalImageUrl = imageUrl;

        if (image != null && image.Length > 0)
        {
            // Delete the old image before saving the new one
            if (!string.IsNullOrEmpty(imageUrl))
                _fileService.DeleteFile(imageUrl);

            using var stream = image.OpenReadStream();
            finalImageUrl = await _fileService.SaveFileAsync(stream, image.FileName, "items");
        }

        var command = new UpdateItemCommand
        {
            ItemId = id,
            Name = name,
            Description = description,
            SKU = sku,
            BasePrice = basePrice,
            ColorId = colorId,
            ImageUrl = finalImageUrl
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

    #region Private Helpers

    private string BuildBaseUrl()
    {
        var request = HttpContext.Request;
        return $"{request.Scheme}://{request.Host}";
    }

    private void ResolveImageUrl(ItemDto dto)
    {
        if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl.StartsWith('/'))
        {
            dto.ImageUrl = $"{BuildBaseUrl()}{dto.ImageUrl}";
        }
        // Cloudinary URLs are already absolute — no transformation needed
    }

    private void ResolveImageUrls(IEnumerable<ItemDto> items)
    {
        var baseUrl = BuildBaseUrl();
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.ImageUrl) && item.ImageUrl.StartsWith('/'))
            {
                item.ImageUrl = $"{baseUrl}{item.ImageUrl}";
            }
        }
    }

    #endregion
}

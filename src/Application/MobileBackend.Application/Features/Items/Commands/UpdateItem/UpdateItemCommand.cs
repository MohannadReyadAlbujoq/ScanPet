using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Items.Commands.UpdateItem;

/// <summary>
/// Command to update an existing item
/// </summary>
public class UpdateItemCommand : IRequest<Result<bool>>
{
    public Guid ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal BasePrice { get; set; }
    public Guid? ColorId { get; set; }
    public string? ImageUrl { get; set; }
}

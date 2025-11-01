using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Items.Commands.CreateItem;

/// <summary>
/// Command to create a new item
/// </summary>
public class CreateItemCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal BasePrice { get; set; }
    public int Quantity { get; set; } = 0;
    public Guid? ColorId { get; set; }
    public string? ImageUrl { get; set; }
}

using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Discounts.Commands.UpsertDiscount;

public class UpsertDiscountCommand : IRequest<Result<Guid>>
{
    public Guid? Id { get; set; }
    public int Scope { get; set; }
    public Guid ItemId { get; set; }
    public Guid? InventoryId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? Amount { get; set; }
    public string? Label { get; set; }
    public bool IsStackable { get; set; } = true;
    public bool IsRevertable { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

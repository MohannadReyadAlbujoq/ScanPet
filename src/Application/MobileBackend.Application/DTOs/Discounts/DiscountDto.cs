namespace MobileBackend.Application.DTOs.Discounts;

/// <summary>
/// Discount DTO (v5). Returned in item / item-inventory responses and accepted by /api/discounts.
/// </summary>
public class DiscountDto
{
    public Guid? Id { get; set; }

    /// <summary>0=Item, 1=ItemInventory, 2=ItemLocation</summary>
    public int Scope { get; set; }
    public string? ScopeName { get; set; }

    public Guid ItemId { get; set; }
    public Guid? InventoryId { get; set; }
    public Guid? LocationId { get; set; }

    /// <summary>Discount amount per unit. Null when no discount (0 is stored as null). Negative = surcharge.</summary>
    public decimal? Amount { get; set; }

    public string? Label { get; set; }

    /// <summary>When true the discount stacks; when false it is exclusive.</summary>
    public bool IsStackable { get; set; } = true;

    public bool IsRevertable { get; set; } = true;

    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Body for POST /api/discounts and PUT /api/discounts/{id}.
/// </summary>
public class UpsertDiscountRequest
{
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

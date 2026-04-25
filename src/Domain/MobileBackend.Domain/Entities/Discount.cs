using MobileBackend.Domain.Common;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// A monetary discount that can apply to an Item, an ItemInventory, or every
/// ItemInventory in a Location. Stored as a nullable decimal ? callers must
/// store NULL when the amount is zero (or negative-zero). Negative values are
/// allowed and represent a surcharge.
/// </summary>
public class Discount : BaseEntity, ISoftDelete
{
    public DiscountScope Scope { get; set; }

    /// <summary>The Item the discount targets (always required).</summary>
    public Guid ItemId { get; set; }

    /// <summary>Required when Scope = ItemInventory.</summary>
    public Guid? InventoryId { get; set; }

    /// <summary>Required when Scope = ItemLocation.</summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// Discount amount per unit. NULL means no discount. Negative = surcharge.
    /// Callers MUST coerce 0 to NULL.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>Optional human label, e.g. "Spring sale".</summary>
    public string? Label { get; set; }

    /// <summary>
    /// When true, this discount stacks additively with every other stackable discount
    /// hitting the same line. When false, it is exclusive: among the non-stackable
    /// discounts the one with the largest absolute amount wins; stackable discounts
    /// are then summed on top.
    /// </summary>
    public bool IsStackable { get; set; } = true;

    /// <summary>When true, the discount can be reverted by an admin (audit-aware).</summary>
    public bool IsRevertable { get; set; } = true;

    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public virtual Item Item { get; set; } = null!;
    public virtual Inventory? Inventory { get; set; }
    public virtual Location? Location { get; set; }

    /// <summary>True when the discount is currently within its validity window.</summary>
    public bool IsActiveAt(DateTime when) =>
        !IsDeleted
        && Amount.HasValue && Amount.Value != 0m
        && (!StartsAt.HasValue || StartsAt.Value <= when)
        && (!ExpiresAt.HasValue || ExpiresAt.Value >= when);
}

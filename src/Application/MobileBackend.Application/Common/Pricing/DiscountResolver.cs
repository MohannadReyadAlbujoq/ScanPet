using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Common.Pricing;

/// <summary>
/// Snapshot of every discount that applies to a specific item-at-inventory at a point in time,
/// plus the effective per-unit discount after applying stackable/exclusive rules.
/// </summary>
public class DiscountResolution
{
    public List<Discount> Applied { get; set; } = new();

    /// <summary>
    /// Effective per-unit discount. Sum of all stackable discounts plus the largest
    /// (by absolute value) non-stackable discount. Null when result would be 0.
    /// </summary>
    public decimal? PerUnit { get; set; }
}

/// <summary>
/// Applies the v5 discount hierarchy to an item-at-inventory:
///   1. Item-level discounts
///   2. ItemLocation discounts (when the inventory belongs to a location)
///   3. ItemInventory discounts
/// Stackable discounts add up. Among non-stackable discounts the one with the
/// largest absolute amount wins (the rest are ignored). Negative discounts are
/// allowed (surcharge). 0/null amounts are ignored.
/// </summary>
public static class DiscountResolver
{
    public static DiscountResolution Resolve(
        IEnumerable<Discount> candidates,
        Guid itemId,
        Guid inventoryId,
        Guid? locationId,
        DateTime when)
    {
        var applied = candidates
            .Where(d => d.ItemId == itemId && d.IsActiveAt(when) && d.Amount.HasValue && d.Amount.Value != 0m)
            .Where(d =>
                d.Scope == DiscountScope.Item
                || (d.Scope == DiscountScope.ItemInventory && d.InventoryId == inventoryId)
                || (d.Scope == DiscountScope.ItemLocation && locationId.HasValue && d.LocationId == locationId.Value))
            .ToList();

        if (applied.Count == 0)
            return new DiscountResolution { PerUnit = null };

        var stackableSum = applied.Where(d => d.IsStackable).Sum(d => d.Amount!.Value);
        var nonStackable = applied.Where(d => !d.IsStackable).ToList();
        var bestExclusive = nonStackable.Count == 0
            ? 0m
            : nonStackable.OrderByDescending(d => Math.Abs(d.Amount!.Value)).First().Amount!.Value;

        var perUnit = stackableSum + bestExclusive;
        return new DiscountResolution
        {
            Applied = applied,
            PerUnit = perUnit == 0m ? null : perUnit
        };
    }
}

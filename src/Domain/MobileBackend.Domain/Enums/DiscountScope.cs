namespace MobileBackend.Domain.Enums;

/// <summary>
/// Where a discount applies. The combination of Scope + ScopeId points to the target row.
/// </summary>
public enum DiscountScope
{
    /// <summary>Applies to a single Item (any inventory, any location).</summary>
    Item = 0,
    /// <summary>Applies to a specific Item at a specific Inventory.</summary>
    ItemInventory = 1,
    /// <summary>Applies to a specific Item at every Inventory in a specific Location.</summary>
    ItemLocation = 2
}

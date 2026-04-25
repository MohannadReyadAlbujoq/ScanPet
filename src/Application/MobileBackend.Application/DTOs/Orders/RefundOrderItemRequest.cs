namespace MobileBackend.Application.DTOs.Orders;

/// <summary>
/// v5: Refund request now operates at the order level and accepts an array of items.
/// </summary>
public class RefundOrderItemRequest
{
    /// <summary>
    /// Order to refund against.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Inventory (warehouse) where refunded items should be returned.
    /// </summary>
    public Guid RefundToInventoryId { get; set; }

    /// <summary>
    /// Optional reason applied to every refunded line.
    /// </summary>
    public string? RefundReason { get; set; }

    /// <summary>
    /// Items to refund. Each entry references either an OrderItemId or an ItemId plus a quantity.
    /// </summary>
    public List<RefundLineDto> Items { get; set; } = new();
}

public class RefundLineDto
{
    /// <summary>
    /// Order item to refund. If null, ItemId is used to locate the line within the order.
    /// </summary>
    public Guid? OrderItemId { get; set; }

    /// <summary>
    /// Item id (alternative to OrderItemId).
    /// </summary>
    public Guid? ItemId { get; set; }

    /// <summary>
    /// Quantity to refund. Must be greater than zero and not exceed the remaining quantity on the line.
    /// </summary>
    public int Quantity { get; set; }
}

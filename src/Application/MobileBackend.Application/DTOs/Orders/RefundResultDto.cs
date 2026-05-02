namespace MobileBackend.Application.DTOs.Orders;

/// <summary>
/// Result of an order refund operation (v5).
/// </summary>
public class RefundResultDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Resulting overall order status (Refunded, PartiallyRefunded, or unchanged).
    /// </summary>
    public int OrderStatus { get; set; }
    public string OrderStatusName { get; set; } = string.Empty;

    /// <summary>
    /// Aggregate refunded quantity / total ordered quantity (0..100).
    /// </summary>
    public decimal RefundedPercent { get; set; }

    /// <summary>
    /// Aggregate refunded amount across the order.
    /// </summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// Per-line breakdown of what was just refunded.
    /// </summary>
    public List<RefundedLineSummaryDto> RefundedItems { get; set; } = new();
}

public class RefundedLineSummaryDto
{
    public Guid OrderItemId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemImageUrl { get; set; }
    /// <summary>True when the underlying item has been soft-deleted.</summary>
    public bool IsItemDeleted { get; set; }
    public string? SerialNumber { get; set; }
    public int Quantity { get; set; }
    public int TotalRefundedQuantity { get; set; }
    public int OrderedQuantity { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal RefundedPercent { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
}

namespace MobileBackend.Application.DTOs.Orders;

/// <summary>
/// Unified OrderItem DTO for all operations (read, create, update)
/// Represents a line item in an order
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// Order item ID (only for responses, not for create)
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Order ID (only for responses, set by order creation)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Item ID (required for create)
    /// </summary>
    public Guid? ItemId { get; set; }

    /// <summary>
    /// Item name (only in responses)
    /// </summary>
    public string? ItemName { get; set; }

    /// <summary>
    /// Item image URL (only in responses)
    /// </summary>
    public string? ItemImageUrl { get; set; }

    /// <summary>
    /// Quantity ordered (required for create/update)
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Unit price at time of order (required for create)
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Total price for this line item (computed: Quantity * UnitPrice)
    /// </summary>
    public decimal? TotalPrice { get; set; }

    /// <summary>
    /// Order item status (Successful, Refunded)
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Order item status name (only in responses)
    /// </summary>
    public string? StatusName { get; set; }

    /// <summary>
    /// Refunded quantity (only in responses, 0 if not refunded)
    /// </summary>
    public int? RefundedQuantity { get; set; }

    /// <summary>
    /// Refund amount for this item (computed: RefundedQuantity * UnitPrice)
    /// </summary>
    public decimal? RefundedAmount { get; set; }

    /// <summary>
    /// Refunded percent for this line (0..100). Null when nothing refunded.
    /// </summary>
    public decimal? RefundedPercent { get; set; }

    /// <summary>
    /// Refund reason (only in responses)
    /// </summary>
    public string? RefundReason { get; set; }

    /// <summary>
    /// Date when item was refunded (only in responses)
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Creation timestamp (only in responses)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Serial number for the order item — uses the Item SKU
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// True when the underlying item has been soft-deleted (image and image URL will be null).
    /// </summary>
    public bool IsItemDeleted { get; set; }
}

namespace MobileBackend.Application.DTOs.Orders;

/// <summary>
/// Unified Order DTO for all operations (read, create, update)
/// Nullable properties are optional based on operation context
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Order ID (only for responses, not for create)
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Auto-generated order number (only in responses)
    /// Format: ORD-YYYYMMDD-####
    /// </summary>
    public string? OrderNumber { get; set; }
    
    /// <summary>
    /// Client name (required for create/update)
    /// </summary>
    public string? ClientName { get; set; }
    
    /// <summary>
    /// Client phone number (required for create/update)
    /// </summary>
    public string? ClientPhone { get; set; }
    
    /// <summary>
    /// Location ID (required for create/update)
    /// </summary>
    public Guid? LocationId { get; set; }
    
    /// <summary>
    /// Location name (only in responses)
    /// </summary>
    public string? LocationName { get; set; }
    
    /// <summary>
    /// Order description/notes (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Total order amount (computed from order items)
    /// </summary>
    public decimal? TotalAmount { get; set; }
    
    /// <summary>
    /// Order status (Pending, Confirmed, Cancelled)
    /// </summary>
    public int? OrderStatus { get; set; }
    
    /// <summary>
    /// Order status name (only in responses)
    /// </summary>
    public string? OrderStatusName { get; set; }
    
    /// <summary>
    /// Order date (defaults to current time)
    /// </summary>
    public DateTime? OrderDate { get; set; }
    
    /// <summary>
    /// List of order items
    /// </summary>
    public List<OrderItemDto>? OrderItems { get; set; }
    
    /// <summary>
    /// Creation timestamp (only in responses)
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp (only in responses)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

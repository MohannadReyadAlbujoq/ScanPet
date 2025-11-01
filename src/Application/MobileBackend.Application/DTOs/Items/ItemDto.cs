namespace MobileBackend.Application.DTOs.Items;

/// <summary>
/// Unified Item DTO for all operations (read, create, update)
/// Nullable properties are optional based on operation context
/// </summary>
public class ItemDto
{
    /// <summary>
    /// Item ID (only for responses, not for create)
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Item name (required for create/update)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Item description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Stock Keeping Unit (optional)
    /// </summary>
    public string? SKU { get; set; }
    
    /// <summary>
    /// Base price (required for create/update)
    /// </summary>
    public decimal? BasePrice { get; set; }
    
    /// <summary>
    /// Available quantity
    /// </summary>
    public int? Quantity { get; set; }
    
    /// <summary>
    /// Color ID (optional reference)
    /// </summary>
    public Guid? ColorId { get; set; }
    
    /// <summary>
    /// Color name (only in responses)
    /// </summary>
    public string? ColorName { get; set; }
    
    /// <summary>
    /// Image URL (optional)
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Creation timestamp (only in responses)
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp (only in responses)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

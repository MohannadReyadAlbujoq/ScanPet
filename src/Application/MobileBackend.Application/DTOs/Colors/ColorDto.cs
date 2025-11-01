namespace MobileBackend.Application.DTOs.Colors;

/// <summary>
/// Unified Color DTO for all operations (read, create, update)
/// Nullable properties are optional based on operation context
/// </summary>
public class ColorDto
{
    /// <summary>
    /// Color ID (only for responses, not for create)
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Color name (required for create/update)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Red value (0-255) - required for create/update
    /// </summary>
    public int? RedValue { get; set; }
    
    /// <summary>
    /// Green value (0-255) - required for create/update
    /// </summary>
    public int? GreenValue { get; set; }
    
    /// <summary>
    /// Blue value (0-255) - required for create/update
    /// </summary>
    public int? BlueValue { get; set; }
    
    /// <summary>
    /// Computed hex code (only in responses, computed from RGB)
    /// </summary>
    public string? HexCode { get; set; }
    
    /// <summary>
    /// Number of items with this color (only in responses)
    /// </summary>
    public int? ItemCount { get; set; }
    
    /// <summary>
    /// Creation timestamp (only in responses)
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp (only in responses)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.DTOs.Locations;

/// <summary>
/// Unified Location DTO for all operations (read, create, update)
/// Nullable properties are optional based on operation context
/// </summary>
public class LocationDto
{
    /// <summary>
    /// Location ID (only for responses, not for create)
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Location name (required for create/update)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Physical address (optional)
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// City name (optional)
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Country name (optional)
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Postal/ZIP code (optional)
    /// </summary>
    public string? PostalCode { get; set; }
    
    /// <summary>
    /// Whether the location is active
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Number of orders at this location (only in responses)
    /// </summary>
    public int? OrderCount { get; set; }
    
    /// <summary>
    /// Number of inventory sections at this location (only in responses)
    /// </summary>
    public int? SectionCount { get; set; }
    
    /// <summary>
    /// Inventory sections/warehouses within this location (only in detailed responses)
    /// </summary>
    public List<InventoryDto>? Sections { get; set; }
    
    /// <summary>
    /// Creation timestamp (only in responses)
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp (only in responses)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

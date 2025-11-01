using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

public class Color : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public int RedValue { get; set; }   // 0-255
    public int GreenValue { get; set; } // 0-255
    public int BlueValue { get; set; }  // 0-255
    public string? Description { get; set; }
    
    // Computed property - will be stored in database using computed column
    public string HexCode => $"#{RedValue:X2}{GreenValue:X2}{BlueValue:X2}";
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}

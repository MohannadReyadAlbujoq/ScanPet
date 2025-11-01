using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

public class Location : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

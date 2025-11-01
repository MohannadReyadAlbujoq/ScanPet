using MobileBackend.Domain.Common;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Domain.Entities;

public class Order : BaseEntity, ISoftDelete
{
    public string OrderNumber { get; set; } = string.Empty; // Auto-generated: ORD-20250101-0001
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; } = 0;
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual Location Location { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

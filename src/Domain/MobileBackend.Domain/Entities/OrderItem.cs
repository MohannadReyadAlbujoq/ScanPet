using MobileBackend.Domain.Common;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Domain.Entities;

public class OrderItem : BaseEntity, ISoftDelete
{
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
    public string SerialNumber { get; set; } = string.Empty; // Auto-generated: SN-ITEM123-20250101-001
    public int Quantity { get; set; } = 1;
    public decimal SalePrice { get; set; }
    public OrderItemStatus Status { get; set; } = OrderItemStatus.Successful;
    public int RefundedQuantity { get; set; } = 0;
    public DateTime? RefundedAt { get; set; }
    public Guid? RefundedBy { get; set; }
    public string? RefundReason { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
    public virtual Item Item { get; set; } = null!;
}

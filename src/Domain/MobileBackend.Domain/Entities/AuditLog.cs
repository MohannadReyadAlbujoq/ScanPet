using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, Logout, Refund, PasswordReset, etc.
    public string EntityName { get; set; } = string.Empty; // User, Order, Item, etc.
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? AdditionalInfo { get; set; }
    
    // Navigation Properties
    public virtual User? User { get; set; }
}

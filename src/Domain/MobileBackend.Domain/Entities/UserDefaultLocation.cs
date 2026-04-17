using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// Junction table for User's default locations (many-to-many)
/// </summary>
public class UserDefaultLocation : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LocationId { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
}

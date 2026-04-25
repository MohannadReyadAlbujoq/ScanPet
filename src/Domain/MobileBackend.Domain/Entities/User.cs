using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

public class User : BaseEntity, ISoftDelete
{
    [Searchable]
    public string Username { get; set; } = string.Empty;
    [Searchable]
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    [Searchable]
    public string? PhoneNumber { get; set; }
    [Searchable]
    public string? FullName { get; set; }
    public bool IsEnabled { get; set; } = false;
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Profile photo URL (uploaded via FileService). Null when no photo set.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Default inventories assigned to this user
    /// </summary>
    public virtual ICollection<UserDefaultInventory> DefaultInventories { get; set; } = new List<UserDefaultInventory>();

    /// <summary>
    /// Default locations assigned to this user
    /// </summary>
    public virtual ICollection<UserDefaultLocation> DefaultLocations { get; set; } = new List<UserDefaultLocation>();

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

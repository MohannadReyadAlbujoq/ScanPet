using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// Permissions using Bitwise operations for optimal performance.
/// Each permission has a unique bit position (2^0, 2^1, 2^2, etc.)
/// Examples:
/// - ColorCreate = 1 (2^0)
/// - ColorEdit = 2 (2^1)
/// - ColorDelete = 4 (2^2)
/// - ItemCreate = 8 (2^3)
/// - ItemEdit = 16 (2^4)
/// - And so on...
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    /// <summary>
    /// Power of 2 value (1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, etc.)
    /// Supports up to 63 permissions using BIGINT (64 bits)
    /// </summary>
    public long PermissionBit { get; set; }
    
    /// <summary>
    /// Category for grouping permissions (Color, Item, Order, User, Refund, etc.)
    /// </summary>
    public string? Category { get; set; }
    
    public bool IsDeleted { get; set; } = false;
}

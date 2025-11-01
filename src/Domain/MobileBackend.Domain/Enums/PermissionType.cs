namespace MobileBackend.Domain.Enums;

/// <summary>
/// Permission types using power of 2 values for bitwise operations.
/// Each permission is assigned a unique bit position.
/// This allows checking permissions with O(1) complexity using bitwise AND operation.
/// Example: To check if user has ItemCreate permission:
///   bool hasPermission = (userPermissionsBitmask & (long)PermissionType.ItemCreate) == (long)PermissionType.ItemCreate;
/// </summary>
public enum PermissionType : long
{
    // Color Permissions (Bits 0-2)
    ColorCreate = 1L << 0,      // 1
    ColorEdit = 1L << 1,        // 2
    ColorDelete = 1L << 2,      // 4
    ColorView = 1L << 3,        // 8
    
    // Item Permissions (Bits 4-7)
    ItemCreate = 1L << 4,       // 16
    ItemEdit = 1L << 5,         // 32
    ItemDelete = 1L << 6,       // 64
    ItemView = 1L << 7,         // 128
    
    // Order Permissions (Bits 8-12)
    OrderCreate = 1L << 8,      // 256
    OrderView = 1L << 9,        // 512
    OrderEdit = 1L << 10,       // 1024
    OrderConfirm = 1L << 11,    // 2048
    OrderCancel = 1L << 12,     // 4096
    
    // Refund Permissions (Bits 13-14)
    RefundProcess = 1L << 13,   // 8192
    RefundView = 1L << 14,      // 16384
    
    // User Management Permissions (Bits 15-20)
    UserView = 1L << 15,        // 32768
    UserCreate = 1L << 16,      // 65536
    UserEdit = 1L << 17,        // 131072
    UserDelete = 1L << 18,      // 262144
    UserApprove = 1L << 19,     // 524288
    UserEnable = 1L << 20,      // 1048576
    UserDisable = 1L << 21,     // 2097152
    UserResetPassword = 1L << 22, // 4194304
    
    // Location Permissions (Bits 23-25)
    LocationCreate = 1L << 23,  // 8388608
    LocationEdit = 1L << 24,    // 16777216
    LocationDelete = 1L << 25,  // 33554432
    LocationView = 1L << 26,    // 67108864
    
    // Role & Permission Management (Bits 27-30)
    RoleCreate = 1L << 27,      // 134217728
    RoleEdit = 1L << 28,        // 268435456
    RoleDelete = 1L << 29,      // 536870912
    RoleView = 1L << 30,        // 1073741824
    PermissionManage = 1L << 31, // 2147483648
    
    // Audit Log Permissions (Bits 32-33)
    AuditLogView = 1L << 32,    // 4294967296
    AuditLogExport = 1L << 33,  // 8589934592
    
    // System Administration (Bits 34-36)
    SystemSettings = 1L << 34,  // 17179869184
    SystemBackup = 1L << 35,    // 34359738368
    SystemRestore = 1L << 36,   // 68719476736
    
    // Can add more permissions up to bit 63 (9223372036854775808)
}

/// <summary>
/// Helper class for permission operations
/// </summary>
public static class PermissionHelper
{
    /// <summary>
    /// Check if a permission bitmask contains a specific permission
    /// </summary>
    public static bool HasPermission(long permissionsBitmask, PermissionType permission)
    {
        long permissionBit = (long)permission;
        return (permissionsBitmask & permissionBit) == permissionBit;
    }
    
    /// <summary>
    /// Check if a permission bitmask contains any of the specified permissions
    /// </summary>
    public static bool HasAnyPermission(long permissionsBitmask, params PermissionType[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (HasPermission(permissionsBitmask, permission))
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Check if a permission bitmask contains all of the specified permissions
    /// </summary>
    public static bool HasAllPermissions(long permissionsBitmask, params PermissionType[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (!HasPermission(permissionsBitmask, permission))
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Add a permission to a bitmask
    /// </summary>
    public static long AddPermission(long permissionsBitmask, PermissionType permission)
    {
        return permissionsBitmask | (long)permission;
    }
    
    /// <summary>
    /// Remove a permission from a bitmask
    /// </summary>
    public static long RemovePermission(long permissionsBitmask, PermissionType permission)
    {
        return permissionsBitmask & ~(long)permission;
    }
    
    /// <summary>
    /// Combine multiple permissions into a bitmask
    /// </summary>
    public static long CombinePermissions(params PermissionType[] permissions)
    {
        long result = 0;
        foreach (var permission in permissions)
        {
            result |= (long)permission;
        }
        return result;
    }
    
    /// <summary>
    /// Get all permissions from a bitmask
    /// </summary>
    public static List<PermissionType> GetPermissions(long permissionsBitmask)
    {
        var permissions = new List<PermissionType>();
        foreach (PermissionType permission in Enum.GetValues(typeof(PermissionType)))
        {
            if (HasPermission(permissionsBitmask, permission))
            {
                permissions.Add(permission);
            }
        }
        return permissions;
    }
}

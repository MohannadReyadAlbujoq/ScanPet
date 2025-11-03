namespace MobileBackend.Application.Common.Constants;

/// <summary>
/// Constants for audit logging
/// Centralizes all audit-related strings to avoid magic strings
/// </summary>
public static class AuditActions
{
    // Authentication actions
    public const string Login = "Login";
    public const string FailedLogin = "FailedLogin";
    public const string Logout = "Logout";
    public const string UserRegistered = "UserRegistered";
    public const string TokenRefreshed = "TokenRefreshed";
    
    // User management actions
    public const string UserCreated = "UserCreated";
    public const string UserUpdated = "UserUpdated";
    public const string UserDeleted = "UserDeleted";
    public const string UserApproved = "UserApproved";
    public const string UserEnabled = "UserEnabled";
    public const string UserDisabled = "UserDisabled";
    public const string PasswordChanged = "PasswordChanged";
    public const string PasswordReset = "PasswordReset";
    
    // Role actions
    public const string RoleCreated = "RoleCreated";
    public const string RoleUpdated = "RoleUpdated";
    public const string RoleDeleted = "RoleDeleted";
    public const string PermissionsAssigned = "PermissionsAssigned";
    
    // Color actions
    public const string ColorCreated = "ColorCreated";
    public const string ColorUpdated = "ColorUpdated";
    public const string ColorDeleted = "ColorDeleted";
    
    // Location actions
    public const string LocationCreated = "LocationCreated";
    public const string LocationUpdated = "LocationUpdated";
    public const string LocationDeleted = "LocationDeleted";
    
    // Data actions
    public const string ItemCreated = "ItemCreated";
    public const string ItemUpdated = "ItemUpdated";
    public const string ItemDeleted = "ItemDeleted";
    public const string OrderCreated = "OrderCreated";
    public const string OrderUpdated = "OrderUpdated";
    public const string OrderConfirmed = "OrderConfirmed";
    public const string OrderCancelled = "OrderCancelled";
    public const string ItemRefunded = "ItemRefunded";
    public const string OrderItemRefunded = "OrderItemRefunded";
    
    // Inventory actions (NEW)
    public const string InventoryCreated = "InventoryCreated";
    public const string InventoryUpdated = "InventoryUpdated";
    public const string InventoryDeleted = "InventoryDeleted";
    public const string ItemInventorySet = "ItemInventorySet";
    public const string InventoryAdjusted = "InventoryAdjusted";
    public const string InventoryTransferred = "InventoryTransferred";
    
    // Generic actions
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
}

/// <summary>
/// Entity names for audit logging
/// </summary>
public static class EntityNames
{
    public const string User = "User";
    public const string Role = "Role";
    public const string Permission = "Permission";
    public const string RefreshToken = "RefreshToken";
    public const string Color = "Color";
    public const string Location = "Location";
    public const string Item = "Item";
    public const string Order = "Order";
    public const string OrderItem = "OrderItem";
    public const string Inventory = "Inventory"; // NEW
    public const string ItemInventory = "ItemInventory"; // NEW
}

/// <summary>
/// Token-related constants
/// </summary>
public static class TokenConstants
{
    public const int AccessTokenExpiryMinutes = 15;
    public const int RefreshTokenExpiryDays = 7;
}

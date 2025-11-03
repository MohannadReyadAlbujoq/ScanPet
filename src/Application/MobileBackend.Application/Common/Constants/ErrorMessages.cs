namespace MobileBackend.Application.Common.Constants;

/// <summary>
/// Centralized error messages for consistent user experience
/// Provides type-safe, reusable error messages across the application
/// </summary>
public static class ErrorMessages
{
    // ============================================
    // GENERIC CRUD OPERATIONS
    // ============================================
    
    /// <summary>
    /// Returns "EntityName not found" message
    /// </summary>
    public static string NotFound(string entityName) 
        => $"{entityName} not found";

    /// <summary>
    /// Returns "EntityName with this field already exists" message
    /// </summary>
    public static string AlreadyExists(string entityName, string field) 
        => $"{entityName} with this {field} already exists";

    /// <summary>
    /// Returns "An error occurred while operation the entityname" message
    /// </summary>
    public static string OperationFailed(string operation, string entityName) 
        => $"An error occurred while {operation} the {entityName.ToLower()}";

    // ============================================
    // CRUD OPERATION HELPERS
    // ============================================
    
    /// <summary>
    /// Returns "An error occurred while creating the entityname" message
    /// </summary>
    public static string CreateFailed(string entityName) 
        => OperationFailed("creating", entityName);

    /// <summary>
    /// Returns "An error occurred while updating the entityname" message
    /// </summary>
    public static string UpdateFailed(string entityName) 
        => OperationFailed("updating", entityName);

    /// <summary>
    /// Returns "An error occurred while deleting the entityname" message
    /// </summary>
    public static string DeleteFailed(string entityName) 
        => OperationFailed("deleting", entityName);

    // ============================================
    // VALIDATION MESSAGES
    // ============================================
    
    /// <summary>
    /// Returns "Cannot delete entityname. reason" message
    /// </summary>
    public static string CannotDelete(string entityName, string reason) 
        => $"Cannot delete {entityName.ToLower()}. {reason}";

    /// <summary>
    /// Returns "Cannot perform operation: operation" message
    /// </summary>
    public static string InvalidOperation(string operation) 
        => $"Cannot perform operation: {operation}";

    /// <summary>
    /// Returns "EntityName already deleted" message
    /// </summary>
    public static string AlreadyDeleted(string entityName)
        => $"{entityName} is already deleted";

    /// <summary>
    /// Returns "EntityName is disabled" message
    /// </summary>
    public static string EntityDisabled(string entityName)
        => $"{entityName} is disabled";

    // ============================================
    // AUTHENTICATION MESSAGES
    // ============================================
    
    /// <summary>
    /// Invalid username or password
    /// </summary>
    public const string InvalidCredentials = "Invalid username or password";

    /// <summary>
    /// Account is disabled. Please contact administrator
    /// </summary>
    public const string AccountDisabled = "Account is disabled. Please contact administrator";

    /// <summary>
    /// Account is pending approval
    /// </summary>
    public const string AccountNotApproved = "Account is pending approval";

    /// <summary>
    /// Token has expired
    /// </summary>
    public const string TokenExpired = "Token has expired";

    /// <summary>
    /// Invalid token
    /// </summary>
    public const string TokenInvalid = "Invalid token";

    /// <summary>
    /// Refresh token has been revoked
    /// </summary>
    public const string TokenRevoked = "Refresh token has been revoked";

    /// <summary>
    /// User is not authenticated
    /// </summary>
    public const string NotAuthenticated = "User is not authenticated";

    // ============================================
    // AUTHORIZATION MESSAGES
    // ============================================
    
    /// <summary>
    /// You are not authorized to perform this action
    /// </summary>
    public const string Unauthorized = "You are not authorized to perform this action";

    /// <summary>
    /// You do not have sufficient permissions
    /// </summary>
    public const string InsufficientPermissions = "You do not have sufficient permissions";

    /// <summary>
    /// Returns "Access denied. Required permission: permissionName" message
    /// </summary>
    public static string PermissionRequired(string permissionName)
        => $"Access denied. Required permission: {permissionName}";

    // ============================================
    // VALIDATION ERROR MESSAGES
    // ============================================
    
    /// <summary>
    /// Validation failed
    /// </summary>
    public const string ValidationFailed = "Validation failed";

    /// <summary>
    /// This field is required
    /// </summary>
    public const string RequiredField = "This field is required";

    /// <summary>
    /// Invalid format
    /// </summary>
    public const string InvalidFormat = "Invalid format";

    /// <summary>
    /// Returns "Field must be between min and max characters" message
    /// </summary>
    public static string LengthRequired(string field, int min, int max)
        => $"{field} must be between {min} and {max} characters";

    /// <summary>
    /// Returns "Field must be at least min characters" message
    /// </summary>
    public static string MinLength(string field, int min)
        => $"{field} must be at least {min} characters";

    /// <summary>
    /// Returns "Field must not exceed max characters" message
    /// </summary>
    public static string MaxLength(string field, int max)
        => $"{field} must not exceed {max} characters";

    /// <summary>
    /// Returns "Field must be a valid value" message
    /// </summary>
    public static string InvalidValue(string field)
        => $"{field} must be a valid value";

    // ============================================
    // USER MANAGEMENT MESSAGES
    // ============================================
    
    /// <summary>
    /// Username is already taken
    /// </summary>
    public const string UsernameExists = "Username is already taken";

    /// <summary>
    /// Email is already registered
    /// </summary>
    public const string EmailExists = "Email is already registered";

    /// <summary>
    /// User not found
    /// </summary>
    public const string UserNotFound = "User not found";

    /// <summary>
    /// Current password is incorrect
    /// </summary>
    public const string InvalidCurrentPassword = "Current password is incorrect";

    /// <summary>
    /// Password must meet security requirements
    /// </summary>
    public const string WeakPassword = "Password must contain at least 8 characters, including uppercase, lowercase, number, and special character";

    // ============================================
    // ROLE & PERMISSION MESSAGES
    // ============================================
    
    /// <summary>
    /// Role not found
    /// </summary>
    public const string RoleNotFound = "Role not found";

    /// <summary>
    /// Returns "Role is assigned to count user(s)" message
    /// </summary>
    public static string RoleInUse(int userCount)
        => $"Cannot delete role. It is assigned to {userCount} user(s)";

    /// <summary>
    /// Permission not found
    /// </summary>
    public const string PermissionNotFound = "Permission not found";

    /// <summary>
    /// Returns "Permissions permissions are invalid" message
    /// </summary>
    public static string InvalidPermissions(string permissions)
        => $"Invalid permissions: {permissions}";

    // ============================================
    // ENTITY-SPECIFIC MESSAGES
    // ============================================
    
    /// <summary>
    /// Color not found
    /// </summary>
    public const string ColorNotFound = "Color not found";

    /// <summary>
    /// Item not found
    /// </summary>
    public const string ItemNotFound = "Item not found";

    /// <summary>
    /// Location not found
    /// </summary>
    public const string LocationNotFound = "Location not found";

    /// <summary>
    /// Order not found
    /// </summary>
    public const string OrderNotFound = "Order not found";

    /// <summary>
    /// Order item not found
    /// </summary>
    public const string OrderItemNotFound = "Order item not found";

    /// <summary>
    /// Insufficient inventory
    /// </summary>
    public const string InsufficientInventory = "Insufficient inventory for this item";

    /// <summary>
    /// Returns "Cannot cancel order. Order is already status" message
    /// </summary>
    public static string CannotCancelOrder(string status)
        => $"Cannot cancel order. Order is already {status.ToLower()}";

    /// <summary>
    /// Returns "Cannot refund item. Item is status" message
    /// </summary>
    public static string CannotRefundItem(string status)
        => $"Cannot refund item. Item is {status.ToLower()}";

    // ============================================
    // SYSTEM MESSAGES
    // ============================================
    
    /// <summary>
    /// An unexpected error occurred
    /// </summary>
    public const string UnexpectedError = "An unexpected error occurred. Please try again later";

    /// <summary>
    /// Service is temporarily unavailable
    /// </summary>
    public const string ServiceUnavailable = "Service is temporarily unavailable";

    /// <summary>
    /// Database connection failed
    /// </summary>
    public const string DatabaseError = "Database connection failed. Please try again later";

    /// <summary>
    /// Operation timeout
    /// </summary>
    public const string OperationTimeout = "Operation timeout. Please try again";
}

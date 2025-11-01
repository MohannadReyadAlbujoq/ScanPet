namespace MobileBackend.Application.DTOs.Common;

/// <summary>
/// Represents a validation error for a specific field
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Name of the field that failed validation
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Error message describing the validation failure
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// The value that was attempted (optional)
    /// </summary>
    public object? AttemptedValue { get; set; }

    /// <summary>
    /// Error code for client-side handling
    /// </summary>
    public string? ErrorCode { get; set; }

    public ValidationError()
    {
    }

    public ValidationError(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    public ValidationError(string propertyName, string errorMessage, object? attemptedValue)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
    }
}

namespace MobileBackend.Application.DTOs.Common;

/// <summary>
/// Generic result wrapper for API responses.
/// Provides consistent response structure across all endpoints.
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class Result<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The actual data returned by the operation
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// List of validation errors (if any)
    /// </summary>
    public List<ValidationError>? ValidationErrors { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static Result<T> SuccessResult(T data, int statusCode = 200)
    {
        return new Result<T>
        {
            Success = true,
            Data = data,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Create a failure result
    /// </summary>
    public static Result<T> FailureResult(string errorMessage, int statusCode = 400)
    {
        return new Result<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Create a validation failure result
    /// </summary>
    public static Result<T> ValidationFailureResult(List<ValidationError> errors)
    {
        return new Result<T>
        {
            Success = false,
            ErrorMessage = "Validation failed",
            ValidationErrors = errors,
            StatusCode = 400
        };
    }
}

/// <summary>
/// Non-generic result for operations that don't return data
/// </summary>
public class Result
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ValidationError>? ValidationErrors { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static Result SuccessResult(int statusCode = 200)
    {
        return new Result
        {
            Success = true,
            StatusCode = statusCode
        };
    }

    public static Result FailureResult(string errorMessage, int statusCode = 400)
    {
        return new Result
        {
            Success = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
}

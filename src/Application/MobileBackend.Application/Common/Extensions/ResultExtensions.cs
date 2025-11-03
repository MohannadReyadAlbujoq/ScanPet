using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Common.Extensions;

/// <summary>
/// Extension methods for Result&lt;T&gt; to simplify common patterns
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Returns a not found result if entity is null, otherwise returns success result with entity
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entity">Entity to check</param>
    /// <param name="entityName">Entity name for error message</param>
    /// <returns>Success result with entity if found, failure result if null</returns>
    /// <example>
    /// var color = await _repository.GetByIdAsync(id);
    /// var result = color.EnsureFound("Color");
    /// if (!result.Success) return result;
    /// // Use color safely here
    /// </example>
    public static Result<T> EnsureFound<T>(
        this T? entity, 
        string entityName) where T : class
    {
        if (entity == null)
        {
            return Result<T>.FailureResult($"{entityName} not found", 404);
        }
        return Result<T>.SuccessResult(entity);
    }

    /// <summary>
    /// Returns a not found result for boolean operations if entity is null
    /// Used in update/delete operations that return bool
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entity">Entity to check</param>
    /// <param name="entityName">Entity name for error message</param>
    /// <returns>Success result with true if found, failure result if null</returns>
    /// <example>
    /// var color = await _repository.GetByIdAsync(id);
    /// var notFoundResult = color.EnsureFoundForOperation("Color");
    /// if (!notFoundResult.Success) return notFoundResult;
    /// // Continue with operation
    /// </example>
    public static Result<bool> EnsureFoundForOperation<T>(
        this T? entity, 
        string entityName) where T : class
    {
        if (entity == null)
        {
            return Result<bool>.FailureResult($"{entityName} not found", 404);
        }
        return Result<bool>.SuccessResult(true);
    }

    /// <summary>
    /// Returns a not found result for Guid operations if entity is null
    /// Used in create operations that return Guid
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entity">Entity to check</param>
    /// <param name="entityName">Entity name for error message</param>
    /// <returns>Success result with Guid.Empty if found, failure result if null</returns>
    public static Result<Guid> EnsureFoundForGuidOperation<T>(
        this T? entity, 
        string entityName) where T : class
    {
        if (entity == null)
        {
            return Result<Guid>.FailureResult($"{entityName} not found", 404);
        }
        return Result<Guid>.SuccessResult(Guid.Empty); // Will be replaced with actual ID
    }

    /// <summary>
    /// Ensures entity is not null and returns it, otherwise throws
    /// Use with caution - prefer EnsureFound for better error handling
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entity">Entity to check</param>
    /// <param name="entityName">Entity name for exception message</param>
    /// <returns>The entity if not null</returns>
    /// <exception cref="InvalidOperationException">Thrown if entity is null</exception>
    public static T EnsureNotNull<T>(this T? entity, string entityName) where T : class
    {
        if (entity == null)
        {
            throw new InvalidOperationException($"{entityName} not found");
        }
        return entity;
    }

    /// <summary>
    /// Maps a Result&lt;T&gt; to Result&lt;TOut&gt; using a mapping function
    /// Only applies mapping if original result was successful
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <param name="result">Original result</param>
    /// <param name="mapper">Mapping function</param>
    /// <returns>Mapped result</returns>
    /// <example>
    /// var entityResult = await GetEntityAsync(id);
    /// var dtoResult = entityResult.Map(entity => MapToDto(entity));
    /// return dtoResult;
    /// </example>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result, 
        Func<TIn, TOut> mapper)
    {
        if (!result.Success || result.Data == null)
        {
            return Result<TOut>.FailureResult(result.ErrorMessage, result.StatusCode);
        }

        try
        {
            var mapped = mapper(result.Data);
            return Result<TOut>.SuccessResult(mapped, result.StatusCode);
        }
        catch (Exception ex)
        {
            return Result<TOut>.FailureResult($"Mapping failed: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Combines multiple results into a single result
    /// Returns success only if all results are successful
    /// </summary>
    /// <param name="results">Results to combine</param>
    /// <returns>Combined result</returns>
    public static Result<bool> CombineResults(params Result<bool>[] results)
    {
        var failures = results.Where(r => !r.Success).ToList();
        
        if (failures.Any())
        {
            var errorMessages = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var statusCode = failures.First().StatusCode;
            return Result<bool>.FailureResult(errorMessages, statusCode);
        }

        return Result<bool>.SuccessResult(true);
    }

    /// <summary>
    /// Executes an action if result is successful
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="result">Result to check</param>
    /// <param name="action">Action to execute</param>
    /// <returns>Original result</returns>
    /// <example>
    /// return result.OnSuccess(r => _logger.LogInformation("Operation successful"));
    /// </example>
    public static Result<T> OnSuccess<T>(
        this Result<T> result, 
        Action<T?> action)
    {
        if (result.Success)
        {
            action(result.Data);
        }
        return result;
    }

    /// <summary>
    /// Executes an action if result is failure
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="result">Result to check</param>
    /// <param name="action">Action to execute</param>
    /// <returns>Original result</returns>
    /// <example>
    /// return result.OnFailure(error => _logger.LogError("Operation failed: {Error}", error));
    /// </example>
    public static Result<T> OnFailure<T>(
        this Result<T> result, 
        Action<string> action)
    {
        if (!result.Success)
        {
            action(result.ErrorMessage);
        }
        return result;
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to Result&lt;bool&gt;
    /// </summary>
    /// <typeparam name="T">Original result type</typeparam>
    /// <param name="result">Result to convert</param>
    /// <returns>Boolean result</returns>
    public static Result<bool> ToBoolean<T>(this Result<T> result)
    {
        if (result.Success)
        {
            return Result<bool>.SuccessResult(true, result.StatusCode);
        }
        return Result<bool>.FailureResult(result.ErrorMessage, result.StatusCode);
    }
}

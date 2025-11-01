using FluentValidation;
using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically validates requests using FluentValidation
/// Runs before the handler executes
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // If no validators registered, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        // If validation failed, return validation error response
        if (failures.Any())
        {
            // Check if response type is Result<T>
            var responseType = typeof(TResponse);
            
            if (responseType.IsGenericType && 
                responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Get the T from Result<T>
                var dataType = responseType.GetGenericArguments()[0];
                
                // Create ValidationError list
                var validationErrors = failures.Select(failure => new ValidationError
                {
                    PropertyName = failure.PropertyName,
                    ErrorMessage = failure.ErrorMessage,
                    AttemptedValue = failure.AttemptedValue,
                    ErrorCode = failure.ErrorCode
                }).ToList();

                // Create ValidationFailureResult using reflection
                var validationFailureMethod = typeof(Result<>)
                    .MakeGenericType(dataType)
                    .GetMethod(nameof(Result<object>.ValidationFailureResult));

                var result = validationFailureMethod?.Invoke(null, new object[] { validationErrors });
                
                return (TResponse)result!;
            }
            
            // If not Result<T>, throw validation exception (fallback)
            throw new ValidationException(failures);
        }

        // Validation passed, continue to handler
        return await next();
    }
}

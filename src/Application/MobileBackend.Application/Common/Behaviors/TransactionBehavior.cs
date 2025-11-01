using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for automatic transaction management
/// Wraps handler execution in a database transaction
/// Commits on success, rolls back on exception
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Skip transaction for Query operations (read-only)
        // Only wrap Commands in transactions
        if (!IsCommand(requestName))
        {
            return await next();
        }

        _logger.LogInformation(
            "Beginning transaction for {RequestName}",
            requestName);

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var response = await next();

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Transaction committed for {RequestName}",
                requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Transaction rolled back for {RequestName}",
                requestName);

            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            
            throw;
        }
    }

    /// <summary>
    /// Determine if request is a Command (write operation)
    /// Commands typically have "Command" in the name
    /// Queries typically have "Query" in the name
    /// </summary>
    private static bool IsCommand(string requestName)
    {
        return requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase);
    }
}

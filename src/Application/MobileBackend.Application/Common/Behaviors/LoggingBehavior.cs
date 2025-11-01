using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MobileBackend.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for logging request/response information
/// Logs request details, execution time, and any exceptions
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling {RequestName} - Request: {@Request}", 
            requestName, 
            request);

        try
        {
            var response = await next();
            
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Handled {RequestName} - Time: {ElapsedMilliseconds}ms - Response: {@Response}", 
                requestName, 
                stopwatch.ElapsedMilliseconds,
                response);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(
                ex,
                "Error handling {RequestName} - Time: {ElapsedMilliseconds}ms - Request: {@Request}", 
                requestName, 
                stopwatch.ElapsedMilliseconds,
                request);

            throw;
        }
    }
}

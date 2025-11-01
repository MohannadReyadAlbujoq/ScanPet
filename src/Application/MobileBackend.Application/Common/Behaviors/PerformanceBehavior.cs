using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MobileBackend.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for monitoring performance
/// Logs warnings for slow requests (>500ms by default)
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _stopwatch;
    
    /// <summary>
    /// Threshold in milliseconds for slow request warning
    /// </summary>
    private const int SlowRequestThresholdMs = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _stopwatch = new Stopwatch();
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        _stopwatch.Start();

        var response = await next();

        _stopwatch.Stop();

        var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

        // Log warning if request took too long
        if (elapsedMilliseconds > SlowRequestThresholdMs)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogWarning(
                "Slow Request: {RequestName} took {ElapsedMilliseconds}ms (threshold: {Threshold}ms) - Request: {@Request}",
                requestName,
                elapsedMilliseconds,
                SlowRequestThresholdMs,
                request);
        }

        return response;
    }
}

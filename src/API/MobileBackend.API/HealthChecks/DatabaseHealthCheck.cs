using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.API.HealthChecks;

/// <summary>
/// Health check for database connectivity and responsiveness
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        ApplicationDbContext context,
        ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if database can be connected to
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Check database responsiveness with a simple query
            var startTime = DateTime.UtcNow;
            var userCount = await _context.Users.CountAsync(cancellationToken);
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            var data = new Dictionary<string, object>
            {
                ["database"] = _context.Database.ProviderName ?? "Unknown",
                ["responseTimeMs"] = responseTime,
                ["userCount"] = userCount,
                ["timestamp"] = DateTime.UtcNow
            };

            // Consider degraded if response time > 1 second
            if (responseTime > 1000)
            {
                return HealthCheckResult.Degraded(
                    $"Database responding slowly ({responseTime}ms)",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Database is responsive ({responseTime}ms)",
                data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["timestamp"] = DateTime.UtcNow
                });
        }
    }
}

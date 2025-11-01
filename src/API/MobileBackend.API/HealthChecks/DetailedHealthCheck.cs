using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Reflection;

namespace MobileBackend.API.HealthChecks;

/// <summary>
/// Detailed health check providing system information
/// </summary>
public class DetailedHealthCheck : IHealthCheck
{
    private readonly ILogger<DetailedHealthCheck> _logger;

    public DetailedHealthCheck(ILogger<DetailedHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "Unknown";
            var buildDate = GetBuildDate(assembly);

            var data = new Dictionary<string, object>
            {
                ["version"] = version,
                ["buildDate"] = buildDate,
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                ["machineName"] = Environment.MachineName,
                ["osVersion"] = Environment.OSVersion.ToString(),
                ["processorCount"] = Environment.ProcessorCount,
                ["dotnetVersion"] = Environment.Version.ToString(),
                ["uptime"] = GetUptime(),
                ["timestamp"] = DateTime.UtcNow
            };

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    "API is running",
                    data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Health check failed",
                    ex));
        }
    }

    private static DateTime GetBuildDate(Assembly assembly)
    {
        const string BuildVersionMetadataPrefix = "+build";
        
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value[(index + BuildVersionMetadataPrefix.Length)..];
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", null, 
                    System.Globalization.DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }
        }

        return File.GetLastWriteTime(assembly.Location);
    }

    private static string GetUptime()
    {
        var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
    }
}

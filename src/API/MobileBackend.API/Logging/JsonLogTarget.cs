using NLog;
using NLog.Config;
using NLog.Targets;
using System.Text.Json;

namespace MobileBackend.API.Logging;

/// <summary>
/// JSON log target for structured logging export
/// Enables integration with log analysis tools (ELK, Splunk, etc.)
/// </summary>
[Target("JsonLog")]
public sealed class JsonLogTarget : TargetWithLayout
{
    [RequiredParameter]
    public string FileName { get; set; } = "logs.json";

    public long ArchiveAboveSize { get; set; } = 10_000_000; // 10 MB
    public int MaxArchiveFiles { get; set; } = 50;

    protected override void Write(LogEventInfo logEvent)
    {
        var logEntry = new
        {
            Timestamp = logEvent.TimeStamp,
            Level = logEvent.Level.Name,
            Logger = logEvent.LoggerName,
            Message = logEvent.FormattedMessage,
            Exception = logEvent.Exception?.ToString(),
            Properties = logEvent.Properties.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value?.ToString()
            ),
            ActivityId = GetProperty(logEvent, "ActivityId"),
            Url = GetProperty(logEvent, "Url"),
            RequestType = GetProperty(logEvent, "RequestType"),
            UserName = GetProperty(logEvent, "UserName"),
            Elapsed = GetProperty(logEvent, "Elapsed"),
            MethodName = GetProperty(logEvent, "MethodName"),
            FilePath = GetProperty(logEvent, "FilePath"),
            LineNumber = GetProperty(logEvent, "LineNumber")
        };

        var json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        File.AppendAllLines(FileName, new[] { json });
    }

    private static string? GetProperty(LogEventInfo logEvent, string propertyName)
    {
        return logEvent.Properties.ContainsKey(propertyName)
            ? logEvent.Properties[propertyName]?.ToString()
            : null;
    }
}

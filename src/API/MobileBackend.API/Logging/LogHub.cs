using Microsoft.AspNetCore.SignalR;

namespace MobileBackend.API.Logging;

/// <summary>
/// SignalR Hub for real-time log streaming
/// Enables live log monitoring in browser
/// </summary>
public class LogHub : Hub
{
    private static readonly HashSet<string> _connectedClients = new();

    public override async Task OnConnectedAsync()
    {
        _connectedClients.Add(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectedClients.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public static bool HasConnectedClients() => _connectedClients.Count > 0;

    public async Task SendLogEntry(object logEntry)
    {
        await Clients.All.SendAsync("ReceiveLog", logEntry);
    }
}

/// <summary>
/// SignalR log target for real-time streaming
/// </summary>
[NLog.Targets.Target("SignalRLog")]
public sealed class SignalRLogTarget : NLog.Targets.TargetWithLayout
{
    private readonly IHubContext<LogHub>? _hubContext;

    public SignalRLogTarget(IHubContext<LogHub>? hubContext = null)
    {
        _hubContext = hubContext;
    }

    protected override void Write(NLog.LogEventInfo logEvent)
    {
        // Only send if clients are connected
        if (!LogHub.HasConnectedClients() || _hubContext == null)
        {
            return;
        }

        var logEntry = new
        {
            Timestamp = logEvent.TimeStamp,
            Level = logEvent.Level.Name,
            Message = logEvent.FormattedMessage,
            ActivityId = GetProperty(logEvent, "ActivityId"),
            UserName = GetProperty(logEvent, "UserName")
        };

        _hubContext.Clients.All.SendAsync("ReceiveLog", logEntry);
    }

    private static string? GetProperty(NLog.LogEventInfo logEvent, string propertyName)
    {
        return logEvent.Properties.ContainsKey(propertyName)
            ? logEvent.Properties[propertyName]?.ToString()
            : null;
    }
}

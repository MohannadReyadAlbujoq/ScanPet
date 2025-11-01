# ScanPet HTML Logging System

## Overview
This project implements a sophisticated HTML-based logging system using NLog that creates beautiful, filterable log files similar to the Insighta system.

## Features

### 1. **HTML Log Output**
- Beautiful, styled HTML logs with color-coded log levels
- Each request is grouped in a collapsible block
- Includes request metadata (Date, URL, Method, User, Activity ID, Elapsed Time)

### 2. **Log Filtering**
- Filter buttons: All, Trace, Info, Warning, Error
- Real-time filtering without page reload
- Shows count of visible/total requests

### 3. **File Size Management**
- **Individual file limit**: 5 MB per log file
- **Archive directory**: Old logs moved to `C:\AppLogs\ScanPet\Archive\`
- **Max archive files**: 100 files (approximately 500 MB total)
- **Automatic rotation**: Files rotated with date-based naming (log.2025-01-15.0.html)

### 4. **Log Levels**
- **Trace** (Gray): Detailed diagnostic information
- **Info** (Green): General informational messages
- **Warning** (Orange): Warning messages
- **Error** (Red): Error messages with stack traces

## Configuration

### NLog Configuration (`nlog.config`)
```xml
<variable name="logDirectory" value="C:\AppLogs\ScanPet\"/>
<variable name="archiveDirectory" value="C:\AppLogs\ScanPet\Archive\"/>
<variable name="maxFileSize" value="5000000"/>  <!-- 5 MB -->
<variable name="maxArchiveFiles" value="100"/>   <!-- ~500 MB total -->
```

### Application Settings (`appsettings.json`)
```json
{
  "NLogSettings": {
    "EnableHtmlLogging": true,
    "LogDirectory": "C:\\AppLogs\\ScanPet\\",
    "ArchiveDirectory": "C:\\AppLogs\\ScanPet\\Archive\\",
    "MaxFileSize": 5000000,
    "MaxArchiveFiles": 100,
    "LogLevel": "Trace"
  }
}
```

## Usage

### 1. **Automatic Request Logging**
The `EnhancedLoggingMiddleware` automatically logs all HTTP requests:
- Request start (Trace level)
- Request completion with elapsed time (Info level)
- Errors with stack traces (Error level)

### 2. **Manual Logging**
Use the `ILoggerService` for custom logging:

```csharp
public class MyController : ControllerBase
{
    private readonly ILoggerService _loggerService;

    public MyController(ILoggerService loggerService)
    {
        _loggerService = loggerService;
    }

    public IActionResult MyAction()
    {
        _loggerService.LogTrace("Starting process");
        
        try
        {
            // Your code here
            _loggerService.LogInfo("Process completed successfully");
        }
        catch (Exception ex)
        {
            _loggerService.LogError("Process failed", ex);
            throw;
        }
    }
}
```

### 3. **Using Standard ILogger**
You can also use the standard `ILogger<T>` interface:

```csharp
_logger.LogTrace("Trace message");
_logger.LogInformation("Info message");
_logger.LogWarning("Warning message");
_logger.LogError(exception, "Error message");
```

## Log File Structure

### HTML Structure
```
C:\AppLogs\ScanPet\
??? log.html (Current active log, max 5MB)
??? errors-2025-01-15.log (Error-only log)
??? log-2025-01-15.txt (Text backup)
??? Archive\
    ??? log.2025-01-15.0.html
    ??? log.2025-01-14.0.html
    ??? ... (up to 100 files)
```

### HTML Log Format
Each request block contains:
- **Header** (Purple gradient):
  - Date
  - URL
  - Request Type (GET, POST, etc.)
  - UserName
  - Activity ID (for tracing)
  - Elapsed time in milliseconds

- **Log Table**:
  - Level (Trace/Info/Warning/Error)
  - Date/Time
  - Message
  - Method Name
  - File Path with Line Number

## Log Viewing

### Opening Logs
1. Navigate to `C:\AppLogs\ScanPet\`
2. Open `log.html` in any web browser (Chrome, Firefox, Edge, etc.)
3. Use the filter buttons to view specific log levels

### Filter Options
- **All**: Shows all log entries
- **Trace**: Shows only trace-level logs
- **Info**: Shows only informational logs
- **Warning**: Shows only warning logs
- **Error**: Shows only error logs

## Performance Considerations

### Async Logging
All logging is asynchronous to prevent blocking the main request thread:
```xml
<targets async="true">
  <target xsi:type="AsyncWrapper" name="htmlLogger" overflowAction="Grow">
    ...
  </target>
</targets>
```

### File Locking
- `concurrentWrites="true"` - Multiple threads can write simultaneously
- `keepFileOpen="false"` - File handle released after each write

## Troubleshooting

### Issue: Logs not appearing
**Solution**: 
1. Check if directory exists: `C:\AppLogs\ScanPet\`
2. Verify write permissions
3. Check internal log: `c:\temp\nlog-internal.log`

### Issue: File size limit not working
**Solution**:
1. Verify `archiveAboveSize` is set correctly
2. Check `maxArchiveFiles` is not 0
3. Ensure archive directory exists

### Issue: HTML not formatted
**Solution**:
1. Clear browser cache
2. Check file encoding is UTF-8
3. Verify custom HtmlLog target is loaded

## Advanced Configuration

### Custom Log Directory
To change the log directory, update both files:

**nlog.config**:
```xml
<variable name="logDirectory" value="D:\MyLogs\"/>
```

**appsettings.json**:
```json
"LogDirectory": "D:\\MyLogs\\"
```

### Adjust File Size Limits
```xml
<variable name="maxFileSize" value="10000000"/>  <!-- 10 MB -->
<variable name="maxArchiveFiles" value="50"/>     <!-- 50 files -->
```

### Disable HTML Logging
**appsettings.json**:
```json
"EnableHtmlLogging": false
```

## Architecture

### Components
1. **HtmlLogTarget** (`Logging/HtmlRequestLayoutRenderer.cs`)
   - Custom NLog target
   - Generates HTML output
   - Handles file rotation

2. **EnhancedLoggingMiddleware** (`Logging/EnhancedLoggingMiddleware.cs`)
   - Captures HTTP request/response
   - Tracks activity ID
   - Measures elapsed time

3. **LoggerService** (`Logging/LoggerService.cs`)
   - Injectable service for custom logging
   - Captures caller information automatically
   - Enriches logs with HTTP context

## Best Practices

1. **Use appropriate log levels**:
   - Trace: Very detailed, only for debugging
   - Info: Normal application flow
   - Warning: Unexpected but handled situations
   - Error: Errors and exceptions

2. **Include context**:
   ```csharp
   _loggerService.LogInfo($"Processing order {orderId} for user {userId}");
   ```

3. **Don't log sensitive data**:
   - Passwords are automatically masked
   - Avoid logging credit card numbers, SSNs, etc.

4. **Use structured logging**:
   ```csharp
   _logger.LogInformation("User {UserId} logged in from {IpAddress}", userId, ipAddress);
   ```

## Security Considerations

- Log files may contain sensitive information
- Restrict access to log directory
- Don't expose logs through web server
- Consider encrypting archived logs
- Implement log retention policies

## Maintenance

### Regular Tasks
1. **Monitor disk space**: Check archive folder size
2. **Review error logs**: Analyze patterns in errors-*.log
3. **Archive old logs**: Move ancient logs to long-term storage
4. **Clean up**: Delete unnecessary log files

### Cleanup Script
```powershell
# Delete logs older than 30 days
$path = "C:\AppLogs\ScanPet\Archive\"
$limit = (Get-Date).AddDays(-30)
Get-ChildItem -Path $path -Recurse | 
    Where-Object { $_.LastWriteTime -lt $limit } | 
    Remove-Item -Force
```

## Support

For issues or questions:
1. Check NLog documentation: https://nlog-project.org/
2. Review internal logs: `c:\temp\nlog-internal.log`
3. Enable debug mode in nlog.config:
   ```xml
   internalLogLevel="Debug"
   ```

## License
This logging system is part of the ScanPet Mobile Backend API project.

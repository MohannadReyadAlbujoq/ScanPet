using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Common;
using System.Text;
using System.Collections.Concurrent;

namespace MobileBackend.API.Logging;

/// <summary>
/// Custom NLog target that writes HTML formatted logs with filtering capability
/// Implements file rotation based on size limits and buffered writes for performance
/// </summary>
[Target("HtmlLog")]
public sealed class HtmlLogTarget : TargetWithLayout
{
    private readonly object _lockObject = new();
    private readonly ConcurrentQueue<string> _buffer = new();
    private readonly Timer? _flushTimer;
    private bool _headerWritten;
    private string? _currentActivityId;
    private FileInfo? _currentFile;
    
    // Configurable properties from nlog.config
    [RequiredParameter]
    public string FileName { get; set; } = "log.html";
    
    public string? ArchiveFileName { get; set; }
    public long ArchiveAboveSize { get; set; } = 5_000_000; // 5 MB default
    public int MaxArchiveFiles { get; set; } = 100;
    public string ArchiveDateFormat { get; set; } = "yyyy-MM-dd";
    
    // Buffer settings for performance
    public int BufferSize { get; set; } = 100;
    public int FlushIntervalMs { get; set; } = 1000;

    public HtmlLogTarget()
    {
        // Auto-flush buffer every second
        _flushTimer = new Timer(FlushBuffer, null, FlushIntervalMs, FlushIntervalMs);
    }

    protected override void InitializeTarget()
    {
        base.InitializeTarget();
        
        // Ensure log directory exists
        var directory = Path.GetDirectoryName(FileName);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Ensure archive directory exists
        if (!string.IsNullOrEmpty(ArchiveFileName))
        {
            var archiveDirectory = Path.GetDirectoryName(ArchiveFileName);
            if (!string.IsNullOrEmpty(archiveDirectory) && !Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }
        }

        _currentFile = new FileInfo(FileName);
    }

    protected override void Write(LogEventInfo logEvent)
    {
        var html = FormatLogEvent(logEvent);
        
        // Add to buffer
        _buffer.Enqueue(html);
        
        // Flush if buffer is full
        if (_buffer.Count >= BufferSize)
        {
            FlushBuffer(null);
        }
    }

    private void FlushBuffer(object? state)
    {
        if (_buffer.IsEmpty)
        {
            return;
        }

        lock (_lockObject)
        {
            var sb = new StringBuilder();
            
            while (_buffer.TryDequeue(out var html))
            {
                sb.Append(html);
            }

            if (sb.Length > 0)
            {
                // Check if file needs rotation
                CheckAndRotateFile();
                
                // Write buffered content
                File.AppendAllText(FileName, sb.ToString(), Encoding.UTF8);
                
                // Update file info
                _currentFile?.Refresh();
            }
        }
    }

    protected override void CloseTarget()
    {
        // Flush remaining buffer
        FlushBuffer(null);
        
        lock (_lockObject)
        {
            // Close any open HTML tags
            if (_headerWritten && File.Exists(FileName))
            {
                File.AppendAllText(FileName, "\n            </tbody>\n        </table>\n    </div>\n</body>\n</html>", Encoding.UTF8);
            }
        }
        
        _flushTimer?.Dispose();
        base.CloseTarget();
    }

    private void CheckAndRotateFile()
    {
        if (_currentFile == null || !_currentFile.Exists)
        {
            return;
        }

        _currentFile.Refresh();
        
        if (_currentFile.Length > ArchiveAboveSize)
        {
            RotateFile();
        }
    }

    private void RotateFile()
    {
        try
        {
            // Close HTML tags before rotation
            if (_headerWritten && File.Exists(FileName))
            {
                File.AppendAllText(FileName, "\n            </tbody>\n        </table>\n    </div>\n</body>\n</html>", Encoding.UTF8);
            }

            var archivePath = GenerateArchivePath();
            
            // Move current file to archive
            if (File.Exists(FileName))
            {
                File.Move(FileName, archivePath);
            }

            // Reset state for new file
            _headerWritten = false;
            _currentActivityId = null;
            _currentFile = new FileInfo(FileName);

            // Clean up old archives
            CleanupOldArchives();
        }
        catch (Exception ex)
        {
            InternalLogger.Error(ex, "HtmlLogTarget: Error rotating log file");
        }
    }

    private string GenerateArchivePath()
    {
        if (string.IsNullOrEmpty(ArchiveFileName))
        {
            var directory = Path.GetDirectoryName(FileName) ?? "";
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(FileName);
            var extension = Path.GetExtension(FileName);
            var date = DateTime.Now.ToString(ArchiveDateFormat);
            var sequence = 0;

            string archivePath;
            do
            {
                archivePath = Path.Combine(directory, "Archive", $"{fileNameWithoutExt}.{date}.{sequence}{extension}");
                sequence++;
            } while (File.Exists(archivePath));

            return archivePath;
        }

        // Use configured archive file name pattern
        var pattern = ArchiveFileName
            .Replace("{#}", DateTime.Now.ToString(ArchiveDateFormat) + ".0");
        
        return pattern;
    }

    private void CleanupOldArchives()
    {
        if (MaxArchiveFiles <= 0)
        {
            return;
        }

        try
        {
            var directory = string.IsNullOrEmpty(ArchiveFileName)
                ? Path.Combine(Path.GetDirectoryName(FileName) ?? "", "Archive")
                : Path.GetDirectoryName(ArchiveFileName) ?? "";

            if (!Directory.Exists(directory))
            {
                return;
            }

            var pattern = Path.GetFileNameWithoutExtension(FileName) + ".*" + Path.GetExtension(FileName);
            var archiveFiles = Directory.GetFiles(directory, pattern)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            // Delete oldest files if exceeding limit
            foreach (var file in archiveFiles.Skip(MaxArchiveFiles))
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn(ex, $"HtmlLogTarget: Could not delete archive file: {file.FullName}");
                }
            }
        }
        catch (Exception ex)
        {
            InternalLogger.Error(ex, "HtmlLogTarget: Error cleaning up archive files");
        }
    }

    private string FormatLogEvent(LogEventInfo logEvent)
    {
        var sb = new StringBuilder();

        // Write HTML header only once
        if (!_headerWritten)
        {
            sb.Append(GetHtmlHeader());
            _headerWritten = true;
        }

        // Get properties
        var activityId = GetProperty(logEvent, "ActivityId");
        var isNewRequest = activityId != null && activityId != _currentActivityId;

        if (isNewRequest)
        {
            // Close previous request block if exists
            if (_currentActivityId != null)
            {
                sb.Append("            </tbody>\n        </table>\n    </div>\n");
            }

            // Start new request block
            _currentActivityId = activityId;
            sb.Append(GetRequestHeader(logEvent));
        }

        // Add log entry
        sb.Append(GetLogEntry(logEvent));

        return sb.ToString();
    }

    private static string GetHtmlHeader()
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>ScanPet API Logs - Generated {timestamp}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .header-banner {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 5px; margin-bottom: 20px; box-shadow: 0 2px 5px rgba(0,0,0,0.2); }}
        .header-banner h1 {{ font-size: 24px; margin-bottom: 10px; }}
        .header-banner p {{ font-size: 14px; opacity: 0.9; }}
        .filter-bar {{ background-color: white; padding: 15px; margin-bottom: 20px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); position: sticky; top: 0; z-index: 1000; display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; }}
        .filter-buttons {{ display: flex; gap: 10px; flex-wrap: wrap; }}
        .filter-btn {{ padding: 8px 16px; border: none; border-radius: 3px; cursor: pointer; font-size: 14px; font-weight: 500; transition: all 0.3s; }}
        .filter-btn.active {{ box-shadow: inset 0 2px 4px rgba(0,0,0,0.2); transform: scale(0.98); }}
        .filter-btn.all {{ background-color: #2196F3; color: white; }}
        .filter-btn.all:hover {{ background-color: #1976D2; }}
        .filter-btn.trace {{ background-color: #9E9E9E; color: white; }}
        .filter-btn.trace:hover {{ background-color: #757575; }}
        .filter-btn.info {{ background-color: #4CAF50; color: white; }}
        .filter-btn.info:hover {{ background-color: #388E3C; }}
        .filter-btn.error {{ background-color: #f44336; color: white; }}
        .filter-btn.error:hover {{ background-color: #d32f2f; }}
        .filter-btn.warning {{ background-color: #ff9800; color: white; }}
        .filter-btn.warning:hover {{ background-color: #f57c00; }}
        .request-block {{ background-color: white; margin-bottom: 20px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); overflow: hidden; }}
        .request-header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px; }}
        .request-header div {{ margin: 5px 0; font-size: 14px; }}
        .request-header strong {{ font-weight: 600; margin-right: 10px; }}
        .log-table {{ width: 100%; border-collapse: collapse; }}
        .log-table th {{ background-color: #90caf9; color: #1a237e; padding: 12px; text-align: left; font-weight: 600; font-size: 13px; border-bottom: 2px solid #1a237e; }}
        .log-table td {{ padding: 12px; border-bottom: 1px solid #e0e0e0; font-size: 13px; word-break: break-word; vertical-align: top; }}
        .log-table tr:hover {{ background-color: #f5f5f5; }}
        .log-trace {{ background-color: #fafafa; }}
        .log-info, .log-information {{ background-color: #e8f5e9; }}
        .log-error {{ background-color: #ffebee; }}
        .log-warn, .log-warning {{ background-color: #fff3e0; }}
        .log-debug {{ background-color: #e3f2fd; }}
        .level-trace {{ color: #616161; font-weight: 600; }}
        .level-info, .level-information {{ color: #2e7d32; font-weight: 600; }}
        .level-error {{ color: #c62828; font-weight: 600; }}
        .level-warn, .level-warning {{ color: #f57c00; font-weight: 600; }}
        .level-debug {{ color: #1976d2; font-weight: 600; }}
        .message-cell {{ max-width: 500px; white-space: pre-wrap; font-family: 'Courier New', monospace; font-size: 12px; line-height: 1.4; }}
        .hidden {{ display: none !important; }}
        .stats {{ color: #666; font-size: 12px; display: flex; gap: 15px; }}
        .stats span {{ background-color: #f5f5f5; padding: 5px 10px; border-radius: 3px; }}
        .search-box {{ padding: 8px; border: 1px solid #ddd; border-radius: 3px; font-size: 14px; width: 300px; }}
    </style>
    <script>
        function filterLogs(level) {{
            const blocks = document.querySelectorAll('.request-block');
            const buttons = document.querySelectorAll('.filter-btn');
            
            buttons.forEach(btn => btn.classList.remove('active'));
            event.target.classList.add('active');
            
            blocks.forEach(block => {{

                const rows = block.querySelectorAll('.log-table tbody tr');
                let hasVisibleRows = false;
                
                rows.forEach(row => {{
                    if (level === 'all') {{
                        row.classList.remove('hidden');
                        hasVisibleRows = true;
                    }} else {{
                        const rowLevel = row.className.toLowerCase().split(' ').find(c => c.startsWith('log-'))?.replace('log-', '');
                        if (rowLevel === level || (level === 'info' && rowLevel === 'information')) {{
                            row.classList.remove('hidden');
                            hasVisibleRows = true;
                        }} else {{
                            row.classList.add('hidden');
                        }}
                    }}
                }});
                
                if (!hasVisibleRows) {{
                    block.classList.add('hidden');
                }} else {{
                    block.classList.remove('hidden');
                }}
            }});
            
            updateStats();
        }}
        
        function searchLogs() {{
            const searchTerm = document.getElementById('searchBox').value.toLowerCase();
            const blocks = document.querySelectorAll('.request-block');
            
            blocks.forEach(block => {{
                const text = block.textContent.toLowerCase();
                if (searchTerm === '' || text.includes(searchTerm)) {{
                    block.classList.remove('hidden');
                }} else {{
                    block.classList.add('hidden');
                }}
            }});
            
            updateStats();
        }}
        
        function updateStats() {{
            const visible = document.querySelectorAll('.request-block:not(.hidden)').length;
            const total = document.querySelectorAll('.request-block').length;
            const statsEl = document.getElementById('stats');
            if (statsEl) {{
                statsEl.textContent = `${{visible}} / ${{total}} requests`;
            }}
            
            // Count log levels
            const counts = {{
                trace: 0,
                info: 0,
                warning: 0,
                error: 0
            }};
            
            document.querySelectorAll('.request-block:not(.hidden) .log-table tbody tr').forEach(row => {{
                const level = row.className.toLowerCase().replace('log-', '');
                if (level === 'information') counts.info++;
                else if (counts[level] !== undefined) counts[level]++;
            }});
            
            document.getElementById('traceCount').textContent = counts.trace;
            document.getElementById('infoCount').textContent = counts.info;
            document.getElementById('warningCount').textContent = counts.warning;
            document.getElementById('errorCount').textContent = counts.error;
        }}
        
        window.addEventListener('load', function() {{
            updateStats();
        }});
    </script>
</head>
<body>
    <div class='header-banner'>
        <h1>?? ScanPet API Logs</h1>
        <p>Generated: {timestamp} | Log file with request tracking and performance metrics</p>
    </div>
    <div class='filter-bar'>
        <div class='filter-buttons'>
            <button class='filter-btn all active' onclick='filterLogs(""all"")'>All</button>
            <button class='filter-btn trace' onclick='filterLogs(""trace"")'>Trace (<span id='traceCount'>0</span>)</button>
            <button class='filter-btn info' onclick='filterLogs(""info"")'>Info (<span id='infoCount'>0</span>)</button>
            <button class='filter-btn warning' onclick='filterLogs(""warning"")'>Warning (<span id='warningCount'>0</span>)</button>
            <button class='filter-btn error' onclick='filterLogs(""error"")'>Error (<span id='errorCount'>0</span>)</button>
        </div>
        <div>
            <input type='text' id='searchBox' class='search-box' placeholder='Search logs...' onkeyup='searchLogs()' />
        </div>
        <div class='stats'>
            <span id='stats'>Loading...</span>
        </div>
    </div>
";
    }

    private static string GetRequestHeader(LogEventInfo logEvent)
    {
        var date = logEvent.TimeStamp.ToString("dd/MM/yyyy HH:mm:ss");
        var url = GetProperty(logEvent, "Url") ?? "N/A";
        var requestType = GetProperty(logEvent, "RequestType") ?? "N/A";
        var userName = GetProperty(logEvent, "UserName") ?? "Anonymous";
        var activityId = GetProperty(logEvent, "ActivityId") ?? Guid.NewGuid().ToString();
        var elapsed = GetProperty(logEvent, "Elapsed") ?? "0";

        return $@"
    <div class='request-block'>
        <div class='request-header'>
            <div><strong>Date =</strong> {System.Security.SecurityElement.Escape(date)}</div>
            <div><strong>URL =</strong> {System.Security.SecurityElement.Escape(url)}</div>
            <div><strong>RequestType =</strong> {System.Security.SecurityElement.Escape(requestType)}</div>
            <div><strong>UserName =</strong> {System.Security.SecurityElement.Escape(userName)}</div>
            <div><strong>Activity Id =</strong> {System.Security.SecurityElement.Escape(activityId)}</div>
            <div><strong>Elapsed =</strong> {System.Security.SecurityElement.Escape(elapsed)} ms</div>
        </div>
        <table class='log-table'>
            <thead>
                <tr>
                    <th style='width: 80px;'>Level</th>
                    <th style='width: 150px;'>Date</th>
                    <th>Message</th>
                    <th style='width: 200px;'>Method Name</th>
                    <th style='width: 250px;'>File Path</th>
                </tr>
            </thead>
            <tbody>
";
    }

    private static string GetLogEntry(LogEventInfo logEvent)
    {
        var level = logEvent.Level.Name.ToLower();
        var date = logEvent.TimeStamp.ToString("dd/MM/yyyy HH:mm:ss");
        var message = System.Security.SecurityElement.Escape(logEvent.FormattedMessage ?? "");
        
        // Handle exceptions
        if (logEvent.Exception != null)
        {
            message += $" - {System.Security.SecurityElement.Escape(logEvent.Exception.Message)}";
            if (!string.IsNullOrEmpty(logEvent.Exception.StackTrace))
            {
                message += $"\n{System.Security.SecurityElement.Escape(logEvent.Exception.StackTrace)}";
            }
        }

        var methodName = System.Security.SecurityElement.Escape(GetProperty(logEvent, "MethodName") ?? ".ctor");
        var filePath = GetProperty(logEvent, "FilePath") ?? "";
        var lineNumber = GetProperty(logEvent, "LineNumber") ?? "";
        
        var fullPath = !string.IsNullOrEmpty(lineNumber) 
            ? System.Security.SecurityElement.Escape($"{filePath}, Line Number: {lineNumber}")
            : System.Security.SecurityElement.Escape(filePath);

        var levelClass = $"level-{level}";
        var rowClass = $"log-{level}";

        return $@"
                <tr class='{rowClass}'>
                    <td class='{levelClass}'>{level.ToUpper()}</td>
                    <td>{date}</td>
                    <td class='message-cell'>{message}</td>
                    <td>{methodName}</td>
                    <td>{fullPath}</td>
                </tr>
";
    }

    private static string? GetProperty(LogEventInfo logEvent, string propertyName)
    {
        if (logEvent.Properties.ContainsKey(propertyName))
        {
            var value = logEvent.Properties[propertyName];
            return value?.ToString();
        }
        return null;
    }
}

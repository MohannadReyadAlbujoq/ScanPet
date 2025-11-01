# NLog HTML Logging - Quick Start Guide

## ?? Quick Start (5 Minutes)

### Step 1: Create Log Directories

```powershell
# Run this in PowerShell (Administrator)
New-Item -Path "C:\AppLogs\ScanPet" -ItemType Directory -Force
New-Item -Path "C:\AppLogs\ScanPet\Archive" -ItemType Directory -Force

# Set permissions (for IIS/web apps)
icacls "C:\AppLogs\ScanPet" /grant "IIS_IUSRS:(OI)(CI)M" /T
icacls "C:\AppLogs\ScanPet" /grant "Users:(OI)(CI)M" /T
```

### Step 2: Verify Configuration

The project is already configured! Check these files:
- ? `src/API/MobileBackend.API/nlog.config` - Configuration file
- ? `src/API/MobileBackend.API/appsettings.json` - Settings
- ? `src/API/MobileBackend.API/Program.cs` - NLog initialization

### Step 3: Run the Application

```powershell
cd src\API\MobileBackend.API
dotnet run
```

### Step 4: View Logs

1. **Open HTML Log:**
   - Navigate to: `C:\AppLogs\ScanPet\`
   - Open `log.html` in any browser (Chrome, Firefox, Edge)

2. **Filter Logs:**
   - Click buttons: **All** | **Trace** | **Info** | **Warning** | **Error**

3. **Search Logs:**
   - Use the search box to find specific text

---

## ?? Basic Usage Examples

### Example 1: Automatic Logging (No Code Needed)

Every HTTP request is automatically logged!

```
GET http://localhost:5000/api/users/123
? Automatically creates a log entry with:
  - Date/Time
  - URL
  - Method (GET)
  - User (authenticated user)
  - Duration
  - All logs during the request
```

### Example 2: Manual Logging in Controller

```csharp
public class UsersController : ControllerBase
{
    private readonly ILoggerService _loggerService;

    public UsersController(ILoggerService loggerService)
    {
        _loggerService = loggerService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        _loggerService.LogInfo($"Fetching user {id}");
        
        try
        {
            var user = await _userService.GetAsync(id);
            _loggerService.LogInfo($"User {id} retrieved successfully");
            return Ok(user);
        }
        catch (Exception ex)
        {
            _loggerService.LogError($"Failed to get user {id}", ex);
            throw;
        }
    }
}
```

### Example 3: Standard ILogger Usage

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<User> GetAsync(int id)
    {
        _logger.LogDebug("Querying database for user {UserId}", id);
        
        var user = await _context.Users.FindAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            throw new NotFoundException($"User {id} not found");
        }
        
        _logger.LogInformation("Retrieved user {UserId}: {UserName}", id, user.UserName);
        return user;
    }
}
```

---

## ?? HTML Log Features

### Filter Buttons
- **All**: Show all log entries
- **Trace**: Show only trace-level logs (detailed debugging)
- **Info**: Show only informational logs (normal flow)
- **Warning**: Show only warnings (unexpected but handled)
- **Error**: Show only errors (exceptions and failures)

### Search Box
- Type any text to filter requests
- Searches URL, user name, messages, etc.
- Real-time filtering as you type

### Statistics
- Shows visible/total requests
- Updates automatically when filtering

### Request Blocks
Each HTTP request gets its own colored block with:
- **Purple header** with request details
- **Table** with all logs during that request
- **Color-coded rows** by log level

---

## ?? Configuration

### Change Log Directory

**1. Edit `nlog.config`:**
```xml
<variable name="logDirectory" value="D:\MyLogs\"/>
<variable name="archiveDirectory" value="D:\MyLogs\Archive\"/>
```

**2. Edit `appsettings.json`:**
```json
{
  "NLogSettings": {
    "LogDirectory": "D:\\MyLogs\\",
    "ArchiveDirectory": "D:\\MyLogs\\Archive\\"
  }
}
```

**3. Create directories:**
```powershell
New-Item -Path "D:\MyLogs" -ItemType Directory -Force
New-Item -Path "D:\MyLogs\Archive" -ItemType Directory -Force
```

### Change File Size Limits

**Edit `nlog.config`:**
```xml
<!-- 10 MB per file -->
<variable name="maxFileSize" value="10000000"/>

<!-- Keep 50 archive files -->
<variable name="maxArchiveFiles" value="50"/>
```

### Change Log Level (Production)

**Edit `appsettings.Production.json`:**
```json
{
  "NLogSettings": {
    "LogLevel": "Info"  // Info instead of Trace
  }
}
```

**Or edit `nlog.config`:**
```xml
<rules>
  <logger name="*" minlevel="Info" writeTo="htmlLogger" />
</rules>
```

---

## ?? Troubleshooting

### Problem: No logs appearing

**Solution 1: Check directory exists**
```powershell
Test-Path "C:\AppLogs\ScanPet"
```

**Solution 2: Check permissions**
```powershell
icacls "C:\AppLogs\ScanPet"
```

**Solution 3: Enable debug logging**

Edit `nlog.config`:
```xml
<nlog internalLogLevel="Debug" 
      internalLogFile="c:\temp\nlog-internal.log">
```

Check `c:\temp\nlog-internal.log` for errors.

### Problem: HTML not rendering

**Solution 1: Clear browser cache**
- Press `Ctrl + Shift + Delete`
- Clear cached images and files

**Solution 2: Try different browser**
- Test in Chrome, Firefox, or Edge

**Solution 3: Check file encoding**
- Should be UTF-8
- Verify in file properties

### Problem: File not rotating

**Solution 1: Check file size**
```powershell
(Get-Item "C:\AppLogs\ScanPet\log.html").Length
# Should be < 5 MB (5000000 bytes)
```

**Solution 2: Generate more logs**
- Rotation happens on write
- Make some API requests

**Solution 3: Check archive directory**
```powershell
Test-Path "C:\AppLogs\ScanPet\Archive"
```

---

## ?? Need Help?

### Documentation
1. **Complete Guide**: `docs/NLOG_COMPLETE_GUIDE.md` (Comprehensive, 1000+ lines)
2. **Architecture Review**: `docs/NLOG_ARCHITECTURE_REVIEW.md` (Design patterns, refactoring)
3. **Basic README**: `src/API/MobileBackend.API/Logging/README_LOGGING.md` (Quick reference)

### Quick Checks

```powershell
# Check if directory exists
Test-Path "C:\AppLogs\ScanPet"

# Check log file size
(Get-Item "C:\AppLogs\ScanPet\log.html").Length

# List archive files
Get-ChildItem "C:\AppLogs\ScanPet\Archive\"

# Check permissions
icacls "C:\AppLogs\ScanPet"

# Monitor logs in real-time (text file)
Get-Content "C:\AppLogs\ScanPet\log-2025-01-15.txt" -Wait -Tail 50
```

---

## ?? Tips & Tricks

### Tip 1: Open logs quickly
Create a shortcut on your desktop:
```
Target: C:\AppLogs\ScanPet\log.html
```

### Tip 2: Bookmark internal log
Bookmark this file for quick troubleshooting:
```
file:///c:/temp/nlog-internal.log
```

### Tip 3: Use VS Code for text logs
```powershell
code "C:\AppLogs\ScanPet\log-2025-01-15.txt"
```

### Tip 4: PowerShell monitoring
```powershell
# Watch error logs
Get-Content "C:\AppLogs\ScanPet\errors-*.log" -Wait -Tail 10
```

### Tip 5: Search all logs
```powershell
# Find all logs mentioning "user 123"
Get-ChildItem "C:\AppLogs\ScanPet\*.txt" | Select-String "user 123"
```

---

## ? Verification Checklist

After setup, verify:

- [ ] Log directory exists: `C:\AppLogs\ScanPet\`
- [ ] Archive directory exists: `C:\AppLogs\ScanPet\Archive\`
- [ ] Permissions set (write access)
- [ ] Application starts without errors
- [ ] `log.html` is created
- [ ] HTML opens in browser
- [ ] Filter buttons work
- [ ] Search box works
- [ ] Request blocks appear
- [ ] Errors show in red
- [ ] File rotates at 5 MB

---

## ?? You're Ready!

The logging system is now fully configured and ready to use. Every HTTP request will be automatically logged to beautiful HTML files.

**Next Steps:**
1. Make some API requests
2. Open `C:\AppLogs\ScanPet\log.html`
3. Explore the interactive log viewer
4. Read the complete guide for advanced features

---

**Last Updated:** January 2025  
**Project:** ScanPet Mobile Backend API  
**Quick Start Version:** 1.0

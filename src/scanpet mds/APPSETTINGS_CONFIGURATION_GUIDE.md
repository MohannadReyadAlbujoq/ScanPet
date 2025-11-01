# ?? COMPLETE APPSETTINGS CONFIGURATION GUIDE

**Date:** December 2024  
**Purpose:** Complete configuration reference for all environments  
**File:** `appsettings.json` and environment-specific files

---

## ?? CONFIGURATION FILES STRUCTURE

```
src/API/MobileBackend.API/
??? appsettings.json                    # Base configuration
??? appsettings.Development.json        # Development overrides
??? appsettings.Staging.json           # Staging overrides
??? appsettings.Production.json        # Production overrides
??? Keys/
    ??? private.pem                    # JWT private key
    ??? public.pem                     # JWT public key
```

---

## ?? COMPLETE appsettings.json

### Base Configuration (All Environments)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
    }
  },
  
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ScanPetDB;Username=postgres;Password=your_password_here;Include Error Detail=true"
  },
  
  "Jwt": {
    "PrivateKeyPath": "Keys/private.pem",
    "PublicKeyPath": "Keys/public.pem",
    "Issuer": "ScanPetMobileBackend",
    "Audience": "ScanPetMobileApp",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7,
    "ClockSkew": 5
  },
  
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "capacitor://localhost",
      "ionic://localhost",
      "http://localhost"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true,
    "ExposedHeaders": ["Content-Disposition"]
  },
  
  "Security": {
    "BCryptWorkFactor": 12,
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 30,
    "PasswordRequirements": {
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequireNonAlphanumeric": true,
      "MinimumLength": 8
    }
  },
  
  "Pagination": {
    "DefaultPageSize": 10,
    "MaxPageSize": 100,
    "DefaultPageNumber": 1
  },
  
  "Cache": {
    "DefaultExpirationMinutes": 60,
    "SlidingExpirationMinutes": 20,
    "EnableCompression": true
  },
  
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "Window": 60,
    "QueueLimit": 10
  },
  
  "FileUpload": {
    "MaxFileSizeMB": 10,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx"],
    "UploadPath": "Uploads/",
    "EnableVirusScan": false
  },
  
  "Audit": {
    "EnableAuditLogging": true,
    "LogUserAgent": true,
    "LogIPAddress": true,
    "RetentionDays": 90,
    "SensitiveFields": ["Password", "Token", "RefreshToken"]
  },
  
  "HealthChecks": {
    "EnableDetailedErrors": true,
    "TimeoutSeconds": 10,
    "Database": {
      "Enabled": true,
      "CheckInterval": 30
    },
    "Memory": {
      "Enabled": true,
      "ThresholdMB": 1024
    }
  },
  
  "Email": {
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password",
      "EnableSsl": true,
      "FromName": "ScanPet Mobile",
      "FromEmail": "noreply@scanpet.com"
    },
    "Templates": {
      "WelcomeEmail": "Templates/welcome.html",
      "ResetPassword": "Templates/reset-password.html"
    }
  },
  
  "Sms": {
    "Provider": "Twilio",
    "Twilio": {
      "AccountSid": "your_account_sid",
      "AuthToken": "your_auth_token",
      "FromNumber": "+1234567890"
    }
  },
  
  "Storage": {
    "Provider": "Local",
    "Local": {
      "RootPath": "wwwroot/uploads"
    },
    "Azure": {
      "ConnectionString": "",
      "ContainerName": "scanpet-files"
    },
    "AWS": {
      "AccessKey": "",
      "SecretKey": "",
      "BucketName": "scanpet-files",
      "Region": "us-east-1"
    }
  },
  
  "Monitoring": {
    "ApplicationInsights": {
      "InstrumentationKey": "",
      "EnableAdaptiveSampling": true,
      "SamplingPercentage": 100
    },
    "Serilog": {
      "MinimumLevel": "Information",
      "WriteTo": [
        {
          "Name": "Console"
        },
        {
          "Name": "File",
          "Args": {
            "path": "Logs/log-.txt",
            "rollingInterval": "Day",
            "retainedFileCountLimit": 30
          }
        }
      ]
    }
  },
  
  "Features": {
    "EnableSwagger": true,
    "EnableResponseCompression": true,
    "EnableCors": true,
    "EnableRateLimiting": true,
    "EnableCaching": true,
    "EnableHealthChecks": true
  },
  
  "Database": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "MaxRetryCount": 3,
    "MaxRetryDelay": 30
  }
}
```

---

## ?? appsettings.Development.json

### Development Environment Overrides

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ScanPetDB_Dev;Username=postgres;Password=dev_password;Include Error Detail=true"
  },
  
  "Jwt": {
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  },
  
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "http://localhost:4200",
      "capacitor://localhost",
      "ionic://localhost"
    ]
  },
  
  "Security": {
    "BCryptWorkFactor": 4,
    "MaxLoginAttempts": 999
  },
  
  "Database": {
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  },
  
  "Features": {
    "EnableSwagger": true,
    "EnableResponseCompression": false,
    "EnableRateLimiting": false
  },
  
  "HealthChecks": {
    "EnableDetailedErrors": true
  },
  
  "Audit": {
    "RetentionDays": 7
  }
}
```

---

## ?? appsettings.Staging.json

### Staging Environment Overrides

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=staging-db.example.com;Port=5432;Database=ScanPetDB_Staging;Username=scanpet_user;Password=${DB_PASSWORD};SSL Mode=Require"
  },
  
  "Jwt": {
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  
  "Cors": {
    "AllowedOrigins": [
      "https://staging.scanpet.com",
      "https://staging-app.scanpet.com"
    ]
  },
  
  "Security": {
    "BCryptWorkFactor": 12,
    "MaxLoginAttempts": 5
  },
  
  "Database": {
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false
  },
  
  "Features": {
    "EnableSwagger": true,
    "EnableResponseCompression": true,
    "EnableRateLimiting": true
  },
  
  "Monitoring": {
    "ApplicationInsights": {
      "InstrumentationKey": "${APPINSIGHTS_KEY}",
      "EnableAdaptiveSampling": true
    }
  }
}
```

---

## ?? appsettings.Production.json

### Production Environment Overrides

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db.example.com;Port=5432;Database=ScanPetDB;Username=scanpet_user;Password=${DB_PASSWORD};SSL Mode=Require;Trust Server Certificate=false"
  },
  
  "Jwt": {
    "PrivateKeyPath": "${JWT_PRIVATE_KEY_PATH}",
    "PublicKeyPath": "${JWT_PUBLIC_KEY_PATH}",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  
  "Cors": {
    "AllowedOrigins": [
      "https://scanpet.com",
      "https://www.scanpet.com",
      "https://app.scanpet.com"
    ]
  },
  
  "Security": {
    "BCryptWorkFactor": 12,
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 30
  },
  
  "Database": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "MaxRetryCount": 3
  },
  
  "Features": {
    "EnableSwagger": false,
    "EnableResponseCompression": true,
    "EnableRateLimiting": true,
    "EnableCaching": true
  },
  
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "Window": 60
  },
  
  "Monitoring": {
    "ApplicationInsights": {
      "InstrumentationKey": "${APPINSIGHTS_KEY}",
      "EnableAdaptiveSampling": true,
      "SamplingPercentage": 10
    },
    "Serilog": {
      "MinimumLevel": "Warning",
      "WriteTo": [
        {
          "Name": "ApplicationInsights",
          "Args": {
            "instrumentationKey": "${APPINSIGHTS_KEY}"
          }
        },
        {
          "Name": "File",
          "Args": {
            "path": "Logs/log-.txt",
            "rollingInterval": "Day",
            "retainedFileCountLimit": 90
          }
        }
      ]
    }
  },
  
  "Storage": {
    "Provider": "Azure",
    "Azure": {
      "ConnectionString": "${AZURE_STORAGE_CONNECTION}",
      "ContainerName": "scanpet-production"
    }
  },
  
  "Audit": {
    "EnableAuditLogging": true,
    "RetentionDays": 365
  },
  
  "HealthChecks": {
    "EnableDetailedErrors": false
  }
}
```

---

## ?? ENVIRONMENT VARIABLES

### Required Environment Variables (Production)

```bash
# Database
export DB_PASSWORD="your_secure_db_password"
export DB_CONNECTION_STRING="Host=prod-db.example.com;Port=5432;Database=ScanPetDB;Username=scanpet_user;Password=${DB_PASSWORD};SSL Mode=Require"

# JWT Keys
export JWT_PRIVATE_KEY_PATH="/app/keys/private.pem"
export JWT_PUBLIC_KEY_PATH="/app/keys/public.pem"

# Or store keys directly as base64
export JWT_PRIVATE_KEY=$(cat private.pem | base64 -w 0)
export JWT_PUBLIC_KEY=$(cat public.pem | base64 -w 0)

# Monitoring
export APPINSIGHTS_KEY="your_application_insights_key"

# Email
export SMTP_PASSWORD="your_smtp_password"

# Storage
export AZURE_STORAGE_CONNECTION="DefaultEndpointsProtocol=https;AccountName=..."

# Secrets
export ENCRYPTION_KEY="your_32_character_encryption_key"
```

### Setting Environment Variables

**Windows (PowerShell):**
```powershell
$env:DB_PASSWORD = "your_password"
$env:JWT_PRIVATE_KEY_PATH = "C:\Keys\private.pem"
```

**Linux/macOS:**
```bash
export DB_PASSWORD="your_password"
export JWT_PRIVATE_KEY_PATH="/app/keys/private.pem"
```

**Docker:**
```yaml
# docker-compose.yml
environment:
  - DB_PASSWORD=${DB_PASSWORD}
  - JWT_PRIVATE_KEY_PATH=/app/keys/private.pem
  - ASPNETCORE_ENVIRONMENT=Production
```

---

## ?? CONFIGURATION SECTIONS EXPLAINED

### 1. Logging Configuration

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",           // Default log level
    "Microsoft.AspNetCore": "Warning",  // ASP.NET Core logs
    "Microsoft.EntityFrameworkCore": "Warning"  // EF Core logs
  }
}
```

**Log Levels:**
- `Trace` = Most verbose
- `Debug` = Development details
- `Information` = General info
- `Warning` = Warnings
- `Error` = Errors
- `Critical` = Critical errors
- `None` = No logging

### 2. Connection Strings

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ScanPetDB;Username=postgres;Password=password"
}
```

**PostgreSQL Parameters:**
- `Host` = Database server
- `Port` = Default 5432
- `Database` = Database name
- `Username` = Database user
- `Password` = User password
- `SSL Mode` = Require (production)
- `Include Error Detail` = true (development only)

### 3. JWT Configuration

```json
"Jwt": {
  "PrivateKeyPath": "Keys/private.pem",
  "PublicKeyPath": "Keys/public.pem",
  "Issuer": "ScanPetMobileBackend",
  "Audience": "ScanPetMobileApp",
  "AccessTokenExpiryMinutes": 15,
  "RefreshTokenExpiryDays": 7
}
```

**Parameters:**
- `PrivateKeyPath` = Path to RSA private key
- `PublicKeyPath` = Path to RSA public key
- `Issuer` = Who issued the token
- `Audience` = Who can use the token
- `AccessTokenExpiryMinutes` = Short-lived (15 min)
- `RefreshTokenExpiryDays` = Long-lived (7 days)

### 4. CORS Configuration

```json
"Cors": {
  "AllowedOrigins": ["http://localhost:3000"],
  "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
  "AllowedHeaders": ["*"],
  "AllowCredentials": true
}
```

**Security Note:**
- Never use `"*"` for AllowedOrigins in production
- Always specify exact domains
- Enable credentials only if needed

### 5. Security Settings

```json
"Security": {
  "BCryptWorkFactor": 12,           // Higher = slower but more secure
  "MaxLoginAttempts": 5,
  "LockoutDurationMinutes": 30,
  "PasswordRequirements": {
    "MinimumLength": 8
  }
}
```

### 6. Pagination Defaults

```json
"Pagination": {
  "DefaultPageSize": 10,    // Items per page
  "MaxPageSize": 100,       // Maximum allowed
  "DefaultPageNumber": 1
}
```

### 7. Rate Limiting

```json
"RateLimiting": {
  "EnableRateLimiting": true,
  "PermitLimit": 100,       // Max requests
  "Window": 60,             // Per 60 seconds
  "QueueLimit": 10          // Queue size
}
```

### 8. Health Checks

```json
"HealthChecks": {
  "EnableDetailedErrors": false,   // Hide in production
  "TimeoutSeconds": 10,
  "Database": {
    "Enabled": true,
    "CheckInterval": 30
  }
}
```

---

## ?? ACCESSING CONFIGURATION IN CODE

### Strongly-Typed Configuration

```csharp
// 1. Create configuration class
public class JwtSettings
{
    public string PrivateKeyPath { get; set; }
    public string PublicKeyPath { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}

// 2. Register in Program.cs
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// 3. Inject in service
public class MyService
{
    private readonly JwtSettings _jwtSettings;
    
    public MyService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
}
```

### Direct Access

```csharp
// Get single value
var issuer = builder.Configuration["Jwt:Issuer"];

// Get section
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];

// Get connection string
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
```

---

## ??? SECURITY BEST PRACTICES

### DO ?
- ? Use environment variables for secrets
- ? Different passwords per environment
- ? SSL/TLS in production
- ? Strong BCrypt work factor (12+)
- ? Short token expiry times
- ? Specific CORS origins
- ? Disable Swagger in production
- ? Enable rate limiting
- ? Use Azure Key Vault or AWS Secrets Manager

### DON'T ?
- ? Commit secrets to Git
- ? Use default passwords
- ? Allow "*" in CORS
- ? Enable detailed errors in production
- ? Use same config for all environments
- ? Store keys in appsettings.json
- ? Disable SSL in production

---

## ?? CONFIGURATION CHECKLIST

### Development ?
- [ ] Database connection works
- [ ] JWT keys generated
- [ ] Swagger enabled
- [ ] Detailed errors enabled
- [ ] CORS allows localhost
- [ ] Weak BCrypt for speed

### Production ?
- [ ] Database uses SSL
- [ ] JWT keys in secure store
- [ ] Swagger disabled
- [ ] Detailed errors disabled
- [ ] CORS limited to specific domains
- [ ] Strong BCrypt (12+)
- [ ] Rate limiting enabled
- [ ] Monitoring configured
- [ ] Environment variables set
- [ ] Secrets NOT in appsettings.json

---

**Status:** ? **COMPLETE CONFIGURATION GUIDE**  
**Environments:** Development, Staging, Production  
**Security:** ?? **Best Practices Followed**

---

**END OF CONFIGURATION GUIDE**

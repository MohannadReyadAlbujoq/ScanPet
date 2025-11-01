# ?? Complete App Settings Configuration Guide - ScanPet Mobile Backend

## ?? **Table of Contents**

1. [Overview](#overview)
2. [Configuration Hierarchy](#configuration-hierarchy)
3. [Connection Strings](#connection-strings)
4. [JWT Settings](#jwt-settings)
5. [Security Settings](#security-settings)
6. [NLog Settings](#nlog-settings)
7. [CORS Configuration](#cors-configuration)
8. [API Settings](#api-settings)
9. [Environment-Specific Configuration](#environment-specific-configuration)
10. [Secrets Management](#secrets-management)
11. [Best Practices](#best-practices)
12. [Complete Examples](#complete-examples)

---

## ?? **Overview**

This guide provides detailed explanation of every configuration setting in **appsettings.json** for the ScanPet Mobile Backend API.

**Configuration Files:**
- `appsettings.json` - Base configuration (Development)
- `appsettings.Production.json` - Production overrides
- Environment Variables - Runtime overrides

---

## ?? **Configuration Hierarchy**

.NET applies configurations in this order (later overrides earlier):

```
1. appsettings.json (base)
2. appsettings.{Environment}.json
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments
```

---

## ?? **Connection Strings**

### **PostgreSQL Format:**
```
Host={server};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require
```

### **Security Best Practices:**

1. ? **Use Environment Variables in Production**
2. ? **Enable SSL for Production**
3. ? **Use Secrets Manager (Azure Key Vault, AWS Secrets Manager)**
4. ? **Never commit passwords to source control**

---

## ?? **JWT Settings**

### **Generate Keys:**

**PowerShell:**
```powershell
$rsa = [System.Security.Cryptography.RSA]::Create(2048)
$privateKey = [Convert]::ToBase64String($rsa.ExportRSAPrivateKey())
$publicKey = [Convert]::ToBase64String($rsa.ExportRSAPublicKey())
Write-Host "PrivateKey: $privateKey"
Write-Host "PublicKey: $publicKey"
```

### **Token Expiry Recommendations:**

| Environment | Access Token | Refresh Token |
|-------------|--------------|---------------|
| Development | 60 minutes | 30 days |
| Production | 15 minutes | 7 days |
| High Security | 5 minutes | 1 day |

---

## ??? **Security Settings**

### **Password Requirements:**

| Environment | Min Length | Special Chars | Max Attempts | Lockout |
|-------------|------------|---------------|--------------|---------|
| Development | 8 | Optional | 10 | 5 min |
| Production | 12 | Required | 3 | 30 min |
| High Security | 16 | Required | 3 | 60 min |

---

## ?? **NLog Settings**

### **Log Levels:**

| Level | When to Use | Example |
|-------|-------------|---------|
| Trace | Development debugging | Method entry/exit |
| Info | Production events | User logged in |
| Warning | Potential issues | Retry attempt |
| Error | Errors | Exception occurred |

### **Environment-Specific:**

**Development:**
```json
{
  "NLogSettings": {
    "EnableHtmlLogging": true,
    "LogLevel": "Trace",
    "MaxFileSize": 5000000
  }
}
```

**Production:**
```json
{
  "NLogSettings": {
    "EnableHtmlLogging": false,
    "LogLevel": "Info",
    "MaxFileSize": 10000000
  }
}
```

---

## ?? **CORS Configuration**

### **Security Rules:**

1. ? **Never use wildcard (`*`) with credentials**
2. ? **Use HTTPS in production**
3. ? **List only required origins**

**Development:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

**Production:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://scanpet.com",
      "https://app.scanpet.com"
    ]
  }
}
```

---

## ?? **API Settings**

### **Swagger Configuration:**

| Environment | EnableSwagger | Why |
|-------------|---------------|-----|
| Development | `true` | API documentation |
| Staging | `true` | Testing |
| Production | `false` | Security risk |

### **Rate Limiting:**

| API Type | Rate Limit/min | Use Case |
|----------|----------------|----------|
| Public | 60 | Prevent abuse |
| Authenticated | 120 | Normal users |
| Admin | 1000 | Dashboard |

---

## ?? **Environment-Specific Configuration**

### **Development (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MobileBackendDb;Username=postgres;Password=dev123"
  },
  "JwtSettings": {
    "AccessTokenExpiryMinutes": 60
  },
  "SecuritySettings": {
    "PasswordRequirements": {
      "MinimumLength": 8
    },
    "MaxLoginAttempts": 10
  },
  "NLogSettings": {
    "EnableHtmlLogging": true,
    "LogLevel": "Trace"
  },
  "ApiSettings": {
    "EnableSwagger": true
  }
}
```

### **Production (appsettings.Production.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PRODUCTION_CONNECTION_FROM_ENV_VAR"
  },
  "JwtSettings": {
    "PrivateKey": "PRODUCTION_PRIVATE_KEY_FROM_ENV",
    "PublicKey": "PRODUCTION_PUBLIC_KEY_FROM_ENV",
    "AccessTokenExpiryMinutes": 15
  },
  "SecuritySettings": {
    "PasswordRequirements": {
      "MinimumLength": 12
    },
    "MaxLoginAttempts": 3,
    "LockoutDurationMinutes": 30
  },
  "NLogSettings": {
    "EnableHtmlLogging": false,
    "LogDirectory": "/var/data/logs/",
    "LogLevel": "Info"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://scanpet.com",
      "https://app.scanpet.com"
    ]
  },
  "ApiSettings": {
    "EnableSwagger": false,
    "EnableRateLimiting": true
  }
}
```

---

## ?? **Secrets Management**

### **1. Environment Variables**

**Linux/Mac:**
```bash
export ConnectionStrings__DefaultConnection="Host=..."
export JwtSettings__PrivateKey="BASE64_KEY"
```

**Windows PowerShell:**
```powershell
$env:ConnectionStrings__DefaultConnection="Host=..."
$env:JwtSettings__PrivateKey="BASE64_KEY"
```

**Docker:**
```bash
docker run -e ConnectionStrings__DefaultConnection="Host=..." your-image
```

**Docker Compose:**
```yaml
environment:
  - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
  - JwtSettings__PrivateKey=${JWT_PRIVATE_KEY}
```

### **2. User Secrets (Development)**

```bash
# Initialize
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=..."
dotnet user-secrets set "JwtSettings:PrivateKey" "BASE64_KEY"

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"

# Clear all
dotnet user-secrets clear
```

### **3. Azure Key Vault (Production)**

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

### **4. AWS Secrets Manager**

```bash
# Install AWS SDK
dotnet add package AWSSDK.SecretsManager

# Use in Program.cs
builder.Configuration.AddSecretsManager(
    configurator: options =>
    {
        options.SecretFilter = entry => entry.Name.StartsWith("ScanPet_");
    });
```

---

## ? **Best Practices**

### **1. Never Commit Secrets**

**.gitignore:**
```
# Never commit these
appsettings.Production.json  # If it contains secrets
appsettings.Staging.json
*.pfx
*.key
*.pem
.env
```

### **2. Use Different Keys Per Environment**

```json
// Development
{
  "JwtSettings": {
    "PrivateKey": "DEV_PRIVATE_KEY"
  }
}

// Production
{
  "JwtSettings": {
    "PrivateKey": "PROD_PRIVATE_KEY"  // Different key!
  }
}
```

### **3. Rotate Keys Regularly**

- JWT Keys: Every 90 days
- Database Passwords: Every 90 days
- API Keys: Every 180 days

### **4. Validate Configuration on Startup**

```csharp
// Program.cs
var jwtPrivateKey = builder.Configuration["JwtSettings:PrivateKey"];
if (string.IsNullOrEmpty(jwtPrivateKey))
{
    throw new InvalidOperationException("JWT PrivateKey is not configured");
}
```

### **5. Use Separate Databases Per Environment**

```
Development: MobileBackendDb_Dev
Staging: MobileBackendDb_Staging
Production: MobileBackendDb
```

---

## ?? **Complete Examples**

### **Example 1: Development (Local)**

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MobileBackendDb_Dev;Username=postgres;Password=dev123"
  },
  "DatabaseProvider": "PostgreSQL",
  "JwtSettings": {
    "PrivateKey": "DEV_BASE64_PRIVATE_KEY",
    "PublicKey": "DEV_BASE64_PUBLIC_KEY",
    "Issuer": "MobileBackendAPI",
    "Audience": "MobileApp",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  },
  "SecuritySettings": {
    "EnableBitManipulation": true,
    "EnableRsaEncryption": true,
    "PasswordRequirements": {
      "MinimumLength": 8,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": false
    },
    "MaxLoginAttempts": 10,
    "LockoutDurationMinutes": 5
  },
  "NLogSettings": {
    "EnableHtmlLogging": true,
    "LogDirectory": "C:\\Temp\\Logs\\ScanPet\\",
    "ArchiveDirectory": "C:\\Temp\\Logs\\ScanPet\\Archive\\",
    "MaxFileSize": 5000000,
    "MaxArchiveFiles": 10,
    "LogLevel": "Trace"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "http://127.0.0.1:3000"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  },
  "ApiSettings": {
    "EnableSwagger": true,
    "EnableCompression": true,
    "EnableRateLimiting": false,
    "RateLimitPerMinute": 1000,
    "PageSizeDefault": 20,
    "PageSizeMax": 100
  }
}
```

### **Example 2: Production (On-Premises)**

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db.internal;Port=5432;Database=MobileBackendDb;Username=app_user;Password=FROM_ENV;SSL Mode=Require"
  },
  "JwtSettings": {
    "PrivateKey": "FROM_ENV",
    "PublicKey": "FROM_ENV",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "SecuritySettings": {
    "PasswordRequirements": {
      "MinimumLength": 12,
      "RequireSpecialCharacter": true
    },
    "MaxLoginAttempts": 3,
    "LockoutDurationMinutes": 30
  },
  "NLogSettings": {
    "EnableHtmlLogging": false,
    "LogDirectory": "/var/log/scanpet/",
    "ArchiveDirectory": "/var/log/scanpet/archive/",
    "MaxFileSize": 10000000,
    "MaxArchiveFiles": 100,
    "LogLevel": "Info"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error",
      "Microsoft.EntityFrameworkCore": "Error",
      "MobileBackend": "Information"
    }
  },
  "Cors": {
    "AllowedOrigins": [
      "https://scanpet.com",
      "https://www.scanpet.com",
      "https://app.scanpet.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["Content-Type", "Authorization"],
    "AllowCredentials": true
  },
  "ApiSettings": {
    "EnableSwagger": false,
    "EnableCompression": true,
    "EnableRateLimiting": true,
    "RateLimitPerMinute": 60,
    "PageSizeDefault": 20,
    "PageSizeMax": 50
  }
}
```

**Environment Variables (.env):**
```bash
# Database
ConnectionStrings__DefaultConnection="Host=prod-db.internal;Port=5432;Database=MobileBackendDb;Username=app_user;Password=SecureP@ssw0rd123!;SSL Mode=Require"

# JWT
JwtSettings__PrivateKey="MIIEowIBAAKCAQEA..."
JwtSettings__PublicKey="MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA..."

# Email (if needed)
Email__SmtpPassword="your_smtp_app_password"
```

### **Example 3: Production (Cloud - Render/Railway)**

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "FROM_ENV"
  },
  "JwtSettings": {
    "PrivateKey": "FROM_ENV",
    "PublicKey": "FROM_ENV"
  },
  "NLogSettings": {
    "EnableHtmlLogging": false,
    "LogDirectory": "/var/data/logs/",
    "ArchiveDirectory": "/var/data/logs/archive/",
    "LogLevel": "Info"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://scanpet.vercel.app",
      "https://*.vercel.app"
    ]
  },
  "ApiSettings": {
    "EnableSwagger": false
  }
}
```

**Render Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=postgresql://user:pass@host.neon.tech:5432/db?sslmode=require
JwtSettings__PrivateKey=BASE64_PRIVATE_KEY
JwtSettings__PublicKey=BASE64_PUBLIC_KEY
```

### **Example 4: Docker Deployment**

**docker-compose.yml:**
```yaml
version: '3.8'
services:
  api:
    image: scanpet-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - JwtSettings__PrivateKey=${JWT_PRIVATE_KEY}
      - JwtSettings__PublicKey=${JWT_PUBLIC_KEY}
      - NLogSettings__LogDirectory=/var/data/logs/
```

**.env file:**
```bash
DB_CONNECTION=Host=postgres;Port=5432;Database=scanpet_db;Username=app_user;Password=SecurePass123!
JWT_PRIVATE_KEY=MIIEowIBAAKCAQEA...
JWT_PUBLIC_KEY=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...
```

---

## ?? **Configuration Validation**

### **Startup Validation (Program.cs):**

```csharp
// Add after builder.Build()
var app = builder.Build();

// Validate critical configuration
var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("your_password"))
{
    throw new InvalidOperationException("Database connection string is not properly configured");
}

var jwtPrivateKey = app.Configuration["JwtSettings:PrivateKey"];
if (string.IsNullOrEmpty(jwtPrivateKey) || jwtPrivateKey.Contains("BASE64"))
{
    throw new InvalidOperationException("JWT keys are not properly configured");
}

// Log configuration (without secrets)
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Database Provider: {Provider}", app.Configuration["DatabaseProvider"]);
logger.LogInformation("Swagger Enabled: {SwaggerEnabled}", app.Configuration["ApiSettings:EnableSwagger"]);
```

---

## ?? **Configuration Troubleshooting**

### **Common Issues:**

**1. Connection String Not Found:**
```bash
# Check environment variable format (double underscore)
ConnectionStrings__DefaultConnection  # ? Correct
ConnectionStrings:DefaultConnection   # ? Wrong for env vars
```

**2. JWT Keys Not Working:**
```bash
# Verify Base64 encoding
echo $JwtSettings__PrivateKey | base64 --decode | openssl rsa -check

# Check key length
echo $JwtSettings__PrivateKey | base64 --decode | wc -c
# Should be 1192 for 2048-bit key
```

**3. CORS Still Blocked:**
```json
// Check exact origin match (including protocol and port)
{
  "AllowedOrigins": [
    "http://localhost:3000",  // ? Matches http://localhost:3000
    "http://localhost:3001"   // ? Doesn't match http://localhost:3000
  ]
}
```

---

## ? **Configuration Checklist**

### **Before Deployment:**

- [ ] Connection string configured (environment variable)
- [ ] JWT keys generated and stored securely
- [ ] CORS origins updated for production
- [ ] Swagger disabled in production
- [ ] Log directory exists and has write permissions
- [ ] Email credentials configured (if using email)
- [ ] Rate limiting enabled
- [ ] Password requirements appropriate for environment
- [ ] File upload limits configured
- [ ] Cache settings appropriate
- [ ] All secrets removed from appsettings.json files
- [ ] Environment variables documented
- [ ] Configuration validation added to startup

---

**Last Updated:** January 15, 2025  
**Guide Version:** 1.0  
**Target Stack:** .NET 9, PostgreSQL

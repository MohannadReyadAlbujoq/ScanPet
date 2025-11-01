# ??? IIS DEPLOYMENT GUIDE (Windows Server + SQL Server)

**Date:** December 2024  
**Purpose:** Deploy to Windows Server with IIS + PostgreSQL/SQL Server  
**Environment:** On-premises or Windows VPS

---

## ?? DEPLOYMENT OVERVIEW

**What You'll Set Up:**
- ? Windows Server (2019/2022)
- ? IIS 10+ with ASP.NET Core Hosting Bundle
- ? PostgreSQL 14+ (or SQL Server)
- ? SSL Certificate
- ? Application deployed

**Time Required:** 2-3 hours

---

## ?? PREREQUISITES

### Server Requirements

**Minimum Specifications:**
- Windows Server 2019 or 2022
- 2 CPU cores
- 4 GB RAM
- 50 GB disk space
- Static IP address
- Domain name (optional)

**Software Requirements:**
- .NET 8.0 Hosting Bundle
- PostgreSQL 14+ or SQL Server 2019+
- IIS 10+
- URL Rewrite Module
- Visual C++ Redistributable

---

## ?? STEP 1: PREPARE WINDOWS SERVER

### 1.1 Install IIS

```powershell
# Open PowerShell as Administrator

# Install IIS with required features
Install-WindowsFeature -Name Web-Server -IncludeManagementTools
Install-WindowsFeature -Name Web-WebServer
Install-WindowsFeature -Name Web-Common-Http
Install-WindowsFeature -Name Web-Default-Doc
Install-WindowsFeature -Name Web-Dir-Browsing
Install-WindowsFeature -Name Web-Http-Errors
Install-WindowsFeature -Name Web-Static-Content
Install-WindowsFeature -Name Web-Health
Install-WindowsFeature -Name Web-Http-Logging
Install-WindowsFeature -Name Web-Performance
Install-WindowsFeature -Name Web-Stat-Compression
Install-WindowsFeature -Name Web-Security
Install-WindowsFeature -Name Web-Filtering
Install-WindowsFeature -Name Web-App-Dev
Install-WindowsFeature -Name Web-Net-Ext45
Install-WindowsFeature -Name Web-Asp-Net45
Install-WindowsFeature -Name Web-ISAPI-Ext
Install-WindowsFeature -Name Web-ISAPI-Filter

# Verify IIS is running
Get-Service W3SVC | Select-Object Status
```

### 1.2 Install .NET 8.0 Hosting Bundle

```powershell
# Download .NET 8.0 Hosting Bundle
Invoke-WebRequest -Uri "https://download.visualstudio.microsoft.com/download/pr/...dotnet-hosting-8.0-win.exe" -OutFile "$env:TEMP\dotnet-hosting.exe"

# Install
Start-Process -FilePath "$env:TEMP\dotnet-hosting.exe" -ArgumentList "/quiet", "/norestart" -Wait

# Restart IIS
iisreset /restart

# Verify installation
dotnet --list-runtimes
```

### 1.3 Install URL Rewrite Module

```powershell
# Download URL Rewrite
Invoke-WebRequest -Uri "https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi" -OutFile "$env:TEMP\urlrewrite.msi"

# Install
Start-Process msiexec.exe -ArgumentList "/i", "$env:TEMP\urlrewrite.msi", "/quiet", "/norestart" -Wait

# Restart IIS
iisreset
```

---

## ??? STEP 2: INSTALL & CONFIGURE DATABASE

### Option A: PostgreSQL (Recommended)

#### 2.1 Download & Install PostgreSQL

```powershell
# Download PostgreSQL 14
Invoke-WebRequest -Uri "https://sbp.enterprisedb.com/getfile.jsp?fileid=1258478" -OutFile "$env:TEMP\postgresql-14-windows-x64.exe"

# Install (silent mode)
Start-Process -FilePath "$env:TEMP\postgresql-14-windows-x64.exe" -ArgumentList "--mode", "unattended", "--superpassword", "YourSecurePassword123!", "--serverport", "5432" -Wait
```

#### 2.2 Configure PostgreSQL

```powershell
# Add PostgreSQL to PATH
$env:Path += ";C:\Program Files\PostgreSQL\14\bin"
[Environment]::SetEnvironmentVariable("Path", $env:Path, [System.EnvironmentVariableTarget]::Machine)

# Create database
psql -U postgres -c "CREATE DATABASE ScanPetDB;"

# Create user
psql -U postgres -c "CREATE USER scanpetuser WITH PASSWORD 'YourSecurePassword123!';"

# Grant privileges
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE ScanPetDB TO scanpetuser;"

# Configure pg_hba.conf for remote access
Add-Content "C:\Program Files\PostgreSQL\14\data\pg_hba.conf" "host all all 0.0.0.0/0 md5"

# Configure postgresql.conf
(Get-Content "C:\Program Files\PostgreSQL\14\data\postgresql.conf") -replace "#listen_addresses = 'localhost'", "listen_addresses = '*'" | Set-Content "C:\Program Files\PostgreSQL\14\data\postgresql.conf"

# Restart PostgreSQL
Restart-Service postgresql-x64-14
```

#### 2.3 Configure Windows Firewall

```powershell
# Allow PostgreSQL port
New-NetFirewallRule -DisplayName "PostgreSQL" -Direction Inbound -LocalPort 5432 -Protocol TCP -Action Allow
```

### Option B: SQL Server (Alternative)

#### 2.1 Install SQL Server

```powershell
# Download SQL Server 2022 Express
Invoke-WebRequest -Uri "https://go.microsoft.com/fwlink/?linkid=866658" -OutFile "$env:TEMP\SQLServer2022-SSEI-Expr.exe"

# Run installer (GUI required)
Start-Process "$env:TEMP\SQLServer2022-SSEI-Expr.exe"

# Or silent install
Start-Process "$env:TEMP\SQLServer2022-SSEI-Expr.exe" -ArgumentList "/ACTION=Install", "/Q", "/IACCEPTSQLSERVERLICENSETERMS", "/FEATURES=SQLEngine", "/INSTANCENAME=MSSQLSERVER", "/SECURITYMODE=SQL", "/SAPWD=YourSecurePassword123!" -Wait
```

#### 2.2 Create Database

```sql
-- Connect with SQL Server Management Studio or sqlcmd
sqlcmd -S localhost -U sa -P YourSecurePassword123!

-- Create database
CREATE DATABASE ScanPetDB;
GO

-- Create login
CREATE LOGIN scanpetuser WITH PASSWORD = 'YourSecurePassword123!';
GO

-- Use database
USE ScanPetDB;
GO

-- Create user
CREATE USER scanpetuser FOR LOGIN scanpetuser;
GO

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER scanpetuser;
GO
```

---

## ?? STEP 3: PREPARE APPLICATION

### 3.1 Build & Publish Application

**On Development Machine:**

```powershell
# Navigate to API project
cd C:\Projects\ScanPet\src\API\MobileBackend.API

# Publish for deployment
dotnet publish -c Release -o C:\Publish\ScanPet

# Verify output
dir C:\Publish\ScanPet
```

### 3.2 Create Deployment Package

```powershell
# Compress to ZIP
Compress-Archive -Path C:\Publish\ScanPet\* -DestinationPath C:\Publish\ScanPet.zip
```

### 3.3 Transfer to Server

**Using RDP:**
1. Copy `ScanPet.zip` to server (e.g., `C:\inetpub\apps\`)
2. Extract ZIP file

**Using PowerShell Remoting:**
```powershell
# From dev machine
$session = New-PSSession -ComputerName YOUR_SERVER_IP -Credential (Get-Credential)
Copy-Item C:\Publish\ScanPet.zip -Destination C:\inetpub\apps\ -ToSession $session
```

---

## ?? STEP 4: CONFIGURE IIS

### 4.1 Create Application Pool

```powershell
# Import IIS module
Import-Module WebAdministration

# Create Application Pool
New-WebAppPool -Name "ScanPetAppPool"

# Configure Application Pool
Set-ItemProperty IIS:\AppPools\ScanPetAppPool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\ScanPetAppPool -Name startMode -Value "AlwaysRunning"
Set-ItemProperty IIS:\AppPools\ScanPetAppPool -Name processModel.idleTimeout -Value "00:00:00"
Set-ItemProperty IIS:\AppPools\ScanPetAppPool -Name recycling.periodicRestart.time -Value "00:00:00"

# Set identity (use specific user for database access if needed)
Set-ItemProperty IIS:\AppPools\ScanPetAppPool -Name processModel.identityType -Value "ApplicationPoolIdentity"
```

### 4.2 Create Website

```powershell
# Extract application
Expand-Archive -Path C:\inetpub\apps\ScanPet.zip -DestinationPath C:\inetpub\apps\ScanPet -Force

# Create website
New-Website -Name "ScanPet" `
    -PhysicalPath "C:\inetpub\apps\ScanPet" `
    -ApplicationPool "ScanPetAppPool" `
    -Port 80

# Or bind to specific IP
New-Website -Name "ScanPet" `
    -PhysicalPath "C:\inetpub\apps\ScanPet" `
    -ApplicationPool "ScanPetAppPool" `
    -IPAddress "*" `
    -Port 80 `
    -HostHeader "api.scanpet.com"

# Start website
Start-Website -Name "ScanPet"
```

### 4.3 Configure Application Settings

```powershell
# Set environment variable for application
$appPoolPath = "IIS:\AppPools\ScanPetAppPool"
$envVars = @{
    "ASPNETCORE_ENVIRONMENT" = "Production"
}

foreach ($key in $envVars.Keys) {
    $envVar = New-Object Microsoft.Web.Administration.ConfigurationElement("environmentVariable")
    $envVar.SetAttributeValue("name", $key)
    $envVar.SetAttributeValue("value", $envVars[$key])
    
    $envVarsCollection = Get-ItemProperty $appPoolPath -Name "environmentVariables"
    $envVarsCollection.Add($envVar)
}
```

---

## ?? STEP 5: CONFIGURE SSL/HTTPS

### Option A: Let's Encrypt (Free)

#### 5.1 Install Win-ACME

```powershell
# Download Win-ACME
Invoke-WebRequest -Uri "https://github.com/win-acme/win-acme/releases/download/v2.2.6/win-acme.v2.2.6.x64.pluggable.zip" -OutFile "$env:TEMP\win-acme.zip"

# Extract
Expand-Archive -Path "$env:TEMP\win-acme.zip" -DestinationPath "C:\Tools\win-acme"

# Run Win-ACME
cd C:\Tools\win-acme
.\wacs.exe

# Follow prompts:
# 1. Create certificate (N)
# 2. Single binding of an IIS site (1)
# 3. Select your site
# 4. Email address
# 5. Accept terms
```

#### 5.2 Auto-Renewal

```powershell
# Win-ACME creates scheduled task automatically
# Verify task exists
Get-ScheduledTask -TaskName "win-acme*"
```

### Option B: Import Existing Certificate

```powershell
# Import PFX certificate
$certPassword = ConvertTo-SecureString -String "YourCertPassword" -Force -AsPlainText
Import-PfxCertificate -FilePath "C:\Certs\scanpet.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password $certPassword

# Get certificate thumbprint
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.Subject -like "*scanpet*"}
$thumbprint = $cert.Thumbprint

# Bind to website
New-WebBinding -Name "ScanPet" -Protocol "https" -Port 443 -IPAddress "*" -HostHeader "api.scanpet.com"
$binding = Get-WebBinding -Name "ScanPet" -Protocol "https"
$binding.AddSslCertificate($thumbprint, "My")
```

### 5.3 Force HTTPS Redirect

**Edit web.config:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="HTTPS Redirect" stopProcessing="true">
          <match url="(.*)" />
          <conditions>
            <add input="{HTTPS}" pattern="off" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

---

## ?? STEP 6: CONFIGURE APPLICATION

### 6.1 Update appsettings.Production.json

**On Server: C:\inetpub\apps\ScanPet\appsettings.Production.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ScanPetDB;Username=scanpetuser;Password=YourSecurePassword123!;SSL Mode=Prefer"
  },
  
  "Jwt": {
    "PrivateKeyPath": "C:\\Keys\\private.pem",
    "PublicKeyPath": "C:\\Keys\\public.pem",
    "Issuer": "ScanPetMobileBackend",
    "Audience": "ScanPetMobileApp",
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
  
  "Features": {
    "EnableSwagger": false
  },
  
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  }
}
```

### 6.2 Copy JWT Keys

```powershell
# Create keys directory
New-Item -ItemType Directory -Path "C:\Keys" -Force

# Copy keys (from development machine)
Copy-Item "C:\Projects\ScanPet\Keys\private.pem" -Destination "C:\Keys\private.pem"
Copy-Item "C:\Projects\ScanPet\Keys\public.pem" -Destination "C:\Keys\public.pem"

# Set permissions
$acl = Get-Acl "C:\Keys"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\ScanPetAppPool", "Read", "Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\Keys" $acl
```

---

## ??? STEP 7: RUN DATABASE MIGRATIONS

### 7.1 Install EF Core Tools

```powershell
# Install globally
dotnet tool install --global dotnet-ef

# Or update
dotnet tool update --global dotnet-ef
```

### 7.2 Run Migrations

```powershell
# Navigate to application directory
cd C:\inetpub\apps\ScanPet

# Run migrations
dotnet ef database update --connection "Host=localhost;Port=5432;Database=ScanPetDB;Username=scanpetuser;Password=YourSecurePassword123!"

# Verify
psql -U scanpetuser -d ScanPetDB -c "\dt"
```

### 7.3 Seed Initial Data

**Application auto-seeds on first run, or manually:**

```powershell
# Restart application pool
Restart-WebAppPool -Name "ScanPetAppPool"

# Check logs
Get-Content "C:\inetpub\apps\ScanPet\Logs\log-*.txt" -Tail 50
```

---

## ?? STEP 8: CONFIGURE FIREWALL

```powershell
# Allow HTTP
New-NetFirewallRule -DisplayName "HTTP" -Direction Inbound -LocalPort 80 -Protocol TCP -Action Allow

# Allow HTTPS
New-NetFirewallRule -DisplayName "HTTPS" -Direction Inbound -LocalPort 443 -Protocol TCP -Action Allow

# Verify rules
Get-NetFirewallRule -DisplayName "HTTP*"
```

---

## ?? STEP 9: VERIFY DEPLOYMENT

### 9.1 Test Health Endpoint

```powershell
# Test locally
Invoke-WebRequest -Uri "http://localhost/health"

# Test from internet
Invoke-WebRequest -Uri "https://api.scanpet.com/health"
```

### 9.2 Test API Endpoints

```powershell
# Test login
$body = @{
    email = "admin@scanpet.com"
    password = "Admin@123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.scanpet.com/api/auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### 9.3 Check Logs

```powershell
# Application logs
Get-Content "C:\inetpub\apps\ScanPet\Logs\log-*.txt" -Tail 100

# IIS logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC1\*.log" -Tail 100

# Event Viewer
Get-EventLog -LogName Application -Source "ASP.NET Core*" -Newest 50
```

---

## ?? TROUBLESHOOTING

### Common Issues

#### 1. HTTP Error 500.19

**Cause:** web.config missing or corrupted

**Fix:**
```powershell
# Regenerate web.config
dotnet publish -c Release -o C:\inetpub\apps\ScanPet
```

#### 2. Database Connection Failed

**Cause:** Incorrect connection string or firewall

**Fix:**
```powershell
# Test connection
psql -U scanpetuser -h localhost -d ScanPetDB

# Check PostgreSQL service
Get-Service postgresql-x64-14

# Restart if needed
Restart-Service postgresql-x64-14
```

#### 3. 403 Forbidden

**Cause:** IIS AppPool identity lacks permissions

**Fix:**
```powershell
# Grant permissions to application folder
$path = "C:\inetpub\apps\ScanPet"
$acl = Get-Acl $path
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\ScanPetAppPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $path $acl
```

#### 4. Application Won't Start

**Check:**
```powershell
# Enable detailed errors
Set-ItemProperty IIS:\Sites\ScanPet -Name "stdoutLogEnabled" -Value $true
Set-ItemProperty IIS:\Sites\ScanPet -Name "stdoutLogFile" -Value "C:\inetpub\apps\ScanPet\Logs\stdout"

# Restart
Restart-WebAppPool -Name "ScanPetAppPool"

# Check stdout log
Get-Content "C:\inetpub\apps\ScanPet\Logs\stdout*.log"
```

---

## ?? UPDATES & MAINTENANCE

### Deploy New Version

```powershell
# 1. Stop application pool
Stop-WebAppPool -Name "ScanPetAppPool"

# 2. Backup current version
Copy-Item -Path "C:\inetpub\apps\ScanPet" -Destination "C:\Backups\ScanPet_$(Get-Date -Format yyyyMMdd_HHmmss)" -Recurse

# 3. Extract new version
Expand-Archive -Path "C:\Publish\ScanPet_new.zip" -DestinationPath "C:\inetpub\apps\ScanPet" -Force

# 4. Run migrations
cd C:\inetpub\apps\ScanPet
dotnet ef database update

# 5. Start application pool
Start-WebAppPool -Name "ScanPetAppPool"

# 6. Verify
Invoke-WebRequest -Uri "https://api.scanpet.com/health"
```

### Backup Database

```powershell
# PostgreSQL backup
pg_dump -U scanpetuser -h localhost ScanPetDB > "C:\Backups\ScanPetDB_$(Get-Date -Format yyyyMMdd_HHmmss).sql"

# Automate with scheduled task
$action = New-ScheduledTaskAction -Execute "pg_dump" -Argument "-U scanpetuser ScanPetDB > C:\Backups\DB_backup.sql"
$trigger = New-ScheduledTaskTrigger -Daily -At "02:00"
Register-ScheduledTask -TaskName "Backup ScanPet DB" -Action $action -Trigger $trigger
```

---

## ?? MONITORING

### Setup Performance Counters

```powershell
# Monitor application pool
Get-Counter "\Process(w3wp*)\% Processor Time"
Get-Counter "\Process(w3wp*)\Working Set"

# Create custom monitor
$counters = @(
    "\Web Service(_Total)\Current Connections",
    "\ASP.NET Applications(__Total__)\Requests/Sec",
    "\Memory\Available MBytes"
)

Get-Counter -Counter $counters -SampleInterval 5 -MaxSamples 10
```

---

## ? DEPLOYMENT CHECKLIST

### Pre-Deployment:
- [ ] Windows Server ready
- [ ] IIS installed
- [ ] .NET 8.0 Hosting Bundle installed
- [ ] PostgreSQL/SQL Server installed
- [ ] Database created
- [ ] Application published
- [ ] SSL certificate obtained

### Deployment:
- [ ] Application pool created
- [ ] Website created
- [ ] SSL configured
- [ ] Firewall rules added
- [ ] Application settings configured
- [ ] JWT keys copied
- [ ] Migrations run
- [ ] Data seeded

### Post-Deployment:
- [ ] Health endpoint working
- [ ] API endpoints tested
- [ ] HTTPS working
- [ ] Logs accessible
- [ ] Backups scheduled
- [ ] Monitoring configured

---

**Status:** ? **COMPLETE IIS DEPLOYMENT GUIDE**  
**Platform:** Windows Server + IIS  
**Database:** PostgreSQL or SQL Server  
**SSL:** Configured

---

**END OF IIS DEPLOYMENT GUIDE**

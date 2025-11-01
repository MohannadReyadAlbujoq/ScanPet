# ??? On-Premises Server Deployment Guide - ScanPet Mobile Backend

## ?? **Table of Contents**

1. [Overview](#overview)
2. [Database Deployment - On-Premises](#database-deployment---on-premises)
   - [PostgreSQL on Windows Server](#postgresql-on-windows-server)
   - [PostgreSQL on Linux Server](#postgresql-on-linux-server)
   - [PostgreSQL with Docker](#postgresql-with-docker)
3. [Backend API Deployment - On-Premises](#backend-api-deployment---on-premises)
   - [.NET 9 on Windows Server with IIS](#net-9-on-windows-server-with-iis)
   - [.NET 9 on Linux with Nginx](#net-9-on-linux-with-nginx)
   - [.NET 9 with Docker](#net-9-with-docker)
   - [.NET 9 as Windows Service](#net-9-as-windows-service)
   - [.NET 9 as Linux Systemd Service](#net-9-as-linux-systemd-service)
4. [Security Configuration](#security-configuration)
5. [SSL/TLS Certificate Setup](#ssltls-certificate-setup)
6. [Firewall Configuration](#firewall-configuration)
7. [Monitoring & Maintenance](#monitoring--maintenance)
8. [Troubleshooting](#troubleshooting)

---

## ?? **Overview**

This guide covers deploying ScanPet Mobile Backend API on your **own servers** (Windows or Linux).

**Why On-Premises?**
- ? Full control over infrastructure
- ? No monthly cloud costs
- ? Data sovereignty compliance
- ? Custom security policies
- ? Predictable performance

**What You'll Need:**
- Windows Server 2019+ OR Linux Server (Ubuntu 20.04+/CentOS 8+)
- 4GB RAM minimum (8GB recommended)
- 50GB storage minimum
- Static IP address or domain name
- Administrative access

---

## ??? **Database Deployment - On-Premises**

### **PostgreSQL on Windows Server**

#### **Step 1: Install PostgreSQL**

1. **Download PostgreSQL**
   - Go to: https://www.postgresql.org/download/windows/
   - Download PostgreSQL 16.x (latest stable)
   - Run installer: `postgresql-16.x-windows-x64.exe`

2. **Installation Wizard**
   ```
   Installation Directory: C:\Program Files\PostgreSQL\16
   Data Directory: C:\Program Files\PostgreSQL\16\data
   Port: 5432
   Locale: [Default - English, United States]
   Password: [Strong password for postgres user]
   ```

3. **Complete Installation**
   - Install Stack Builder: ? (optional tools)
   - Launch pgAdmin 4: ?

#### **Step 2: Configure PostgreSQL**

1. **Edit `postgresql.conf`**
   ```
   Location: C:\Program Files\PostgreSQL\16\data\postgresql.conf
   ```

   **Key Settings:**
   ```ini
   # Listen on all interfaces (for remote access)
   listen_addresses = '*'
   
   # Connection limits
   max_connections = 100
   
   # Memory settings (adjust based on your RAM)
   shared_buffers = 256MB
   effective_cache_size = 1GB
   work_mem = 16MB
   maintenance_work_mem = 128MB
   
   # Logging
   log_destination = 'stderr'
   logging_collector = on
   log_directory = 'log'
   log_filename = 'postgresql-%Y-%m-%d_%H%M%S.log'
   log_rotation_age = 1d
   log_rotation_size = 100MB
   
   # Performance
   checkpoint_completion_target = 0.9
   wal_buffers = 16MB
   default_statistics_target = 100
   random_page_cost = 1.1
   effective_io_concurrency = 200
   ```

2. **Edit `pg_hba.conf`** (Client Authentication)
   ```
   Location: C:\Program Files\PostgreSQL\16\data\pg_hba.conf
   ```

   **Add entries:**
   ```conf
   # TYPE  DATABASE        USER            ADDRESS                 METHOD
   
   # Local connections
   local   all             all                                     scram-sha-256
   
   # IPv4 local connections
   host    all             all             127.0.0.1/32            scram-sha-256
   
   # Allow connections from your application server (replace with actual IP)
   host    scanpet_db      scanpet_user    192.168.1.0/24          scram-sha-256
   
   # Or allow from anywhere (less secure, use VPN)
   # host    scanpet_db      scanpet_user    0.0.0.0/0               scram-sha-256
   ```

3. **Restart PostgreSQL Service**
   ```powershell
   Restart-Service postgresql-x64-16
   ```

#### **Step 3: Create Database & User**

1. **Open pgAdmin or psql**
   ```powershell
   # Using psql
   & "C:\Program Files\PostgreSQL\16\bin\psql.exe" -U postgres
   ```

2. **Create Database**
   ```sql
   -- Create database
   CREATE DATABASE scanpet_db
       WITH 
       OWNER = postgres
       ENCODING = 'UTF8'
       LC_COLLATE = 'English_United States.1252'
       LC_CTYPE = 'English_United States.1252'
       TABLESPACE = pg_default
       CONNECTION LIMIT = -1;
   ```

3. **Create User**
   ```sql
   -- Create user
   CREATE USER scanpet_user WITH PASSWORD 'YourStrongPassword123!';
   
   -- Grant privileges
   GRANT ALL PRIVILEGES ON DATABASE scanpet_db TO scanpet_user;
   
   -- Connect to database
   \c scanpet_db
   
   -- Grant schema privileges
   GRANT ALL ON SCHEMA public TO scanpet_user;
   ```

4. **Connection String**
   ```
   Host=your-server-ip;Port=5432;Database=scanpet_db;Username=scanpet_user;Password=YourStrongPassword123!;
   ```

#### **Step 4: Run Migrations**

```powershell
# On your development machine or application server
cd C:\Users\malbujoq\source\repos\ScanPet\src\API\MobileBackend.API

# Update connection string in appsettings.json
# Then run migrations
dotnet ef database update --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj
```

#### **Step 5: Windows Firewall**

```powershell
# Allow PostgreSQL through firewall
New-NetFirewallRule -DisplayName "PostgreSQL" -Direction Inbound -LocalPort 5432 -Protocol TCP -Action Allow
```

---

### **PostgreSQL on Linux Server**

#### **Step 1: Install PostgreSQL (Ubuntu/Debian)**

```bash
# Update package list
sudo apt update

# Install PostgreSQL 16
sudo sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -
sudo apt update
sudo apt install postgresql-16 postgresql-contrib-16 -y

# Check status
sudo systemctl status postgresql
```

#### **Step 2: Configure PostgreSQL**

1. **Edit Configuration**
   ```bash
   sudo nano /etc/postgresql/16/main/postgresql.conf
   ```

   **Key Settings:**
   ```ini
   listen_addresses = '*'
   max_connections = 100
   shared_buffers = 256MB
   effective_cache_size = 1GB
   work_mem = 16MB
   maintenance_work_mem = 128MB
   ```

2. **Edit Authentication**
   ```bash
   sudo nano /etc/postgresql/16/main/pg_hba.conf
   ```

   **Add:**
   ```conf
   # Allow from application server
   host    scanpet_db      scanpet_user    192.168.1.0/24          scram-sha-256
   ```

3. **Restart Service**
   ```bash
   sudo systemctl restart postgresql
   ```

#### **Step 3: Create Database & User**

```bash
# Switch to postgres user
sudo -i -u postgres

# Open psql
psql

# Create database and user
CREATE DATABASE scanpet_db WITH ENCODING='UTF8' LC_COLLATE='en_US.UTF-8' LC_CTYPE='en_US.UTF-8';
CREATE USER scanpet_user WITH PASSWORD 'YourStrongPassword123!';
GRANT ALL PRIVILEGES ON DATABASE scanpet_db TO scanpet_user;

# Exit
\q
exit
```

#### **Step 4: Firewall (UFW)**

```bash
# Allow PostgreSQL
sudo ufw allow 5432/tcp

# Verify
sudo ufw status
```

---

### **PostgreSQL with Docker**

#### **Step 1: Create Docker Compose File**

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: scanpet-postgres
    restart: always
    environment:
      POSTGRES_DB: scanpet_db
      POSTGRES_USER: scanpet_user
      POSTGRES_PASSWORD: YourStrongPassword123!
      PGDATA: /var/lib/postgresql/data/pgdata
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./init-scripts:/docker-entrypoint-initdb.d
    networks:
      - scanpet-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U scanpet_user -d scanpet_db"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres-data:
    driver: local

networks:
  scanpet-network:
    driver: bridge
```

#### **Step 2: Start Container**

```bash
# Start PostgreSQL
docker-compose up -d postgres

# Check logs
docker-compose logs -f postgres

# Verify running
docker ps
```

#### **Step 3: Connection String**

```
Host=localhost;Port=5432;Database=scanpet_db;Username=scanpet_user;Password=YourStrongPassword123!;
```

---

## ??? **Backend API Deployment - On-Premises**

### **.NET 9 on Windows Server with IIS**

#### **Step 1: Install Prerequisites**

1. **Install .NET 9 Hosting Bundle**
   - Download: https://dotnet.microsoft.com/download/dotnet/9.0
   - File: `dotnet-hosting-9.0.x-win.exe`
   - Run installer
   - Restart server: `iisreset` or reboot

2. **Install IIS**
   ```powershell
   # Run in PowerShell (Administrator)
   Install-WindowsFeature -name Web-Server -IncludeManagementTools
   ```

3. **Install URL Rewrite Module** (for HTTPS redirect)
   - Download: https://www.iis.net/downloads/microsoft/url-rewrite
   - Install `rewrite_amd64.msi`

#### **Step 2: Publish Application**

```powershell
# On development machine
cd C:\Users\malbujoq\source\repos\ScanPet\src\API\MobileBackend.API

# Publish for IIS
dotnet publish -c Release -o C:\inetpub\wwwroot\ScanPetAPI

# Or publish to folder, then copy to server
dotnet publish -c Release -o .\publish
# Copy .\publish\* to server: C:\inetpub\wwwroot\ScanPetAPI
```

#### **Step 3: Create IIS Application Pool**

```powershell
# Create Application Pool
Import-Module WebAdministration

New-WebAppPool -Name "ScanPetAPI" -Force

# Configure Application Pool
Set-ItemProperty IIS:\AppPools\ScanPetAPI -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty IIS:\AppPools\ScanPetAPI -Name "enable32BitAppOnWin64" -Value $false
Set-ItemProperty IIS:\AppPools\ScanPetAPI -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
```

#### **Step 4: Create IIS Website**

1. **Using PowerShell:**
   ```powershell
   # Create website
   New-Website -Name "ScanPetAPI" `
       -ApplicationPool "ScanPetAPI" `
       -PhysicalPath "C:\inetpub\wwwroot\ScanPetAPI" `
       -Port 80 `
       -HostHeader "api.scanpet.local"
   
   # Add HTTPS binding (after SSL cert)
   New-WebBinding -Name "ScanPetAPI" -Protocol https -Port 443 -HostHeader "api.scanpet.local" -SslFlags 0
   ```

2. **Or Using IIS Manager:**
   - Open IIS Manager
   - Right-click **Sites** ? **Add Website**
   - Site name: `ScanPetAPI`
   - Application pool: `ScanPetAPI`
   - Physical path: `C:\inetpub\wwwroot\ScanPetAPI`
   - Binding: HTTP, Port 80, Host name: `api.scanpet.local`

#### **Step 5: Configure web.config**

Create/Edit `C:\inetpub\wwwroot\ScanPetAPI\web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\MobileBackend.API.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="ASPNETCORE_HTTPS_PORT" value="443" />
        </environmentVariables>
      </aspNetCore>
      
      <!-- Security headers -->
      <httpProtocol>
        <customHeaders>
          <add name="X-Content-Type-Options" value="nosniff" />
          <add name="X-Frame-Options" value="DENY" />
          <add name="X-XSS-Protection" value="1; mode=block" />
          <add name="Referrer-Policy" value="no-referrer" />
        </customHeaders>
      </httpProtocol>
      
      <!-- Compression -->
      <httpCompression>
        <dynamicTypes>
          <add mimeType="application/json" enabled="true" />
          <add mimeType="application/json; charset=utf-8" enabled="true" />
        </dynamicTypes>
      </httpCompression>
      
      <!-- URL Rewrite (HTTP to HTTPS) -->
      <rewrite>
        <rules>
          <rule name="HTTP to HTTPS redirect" stopProcessing="true">
            <match url="(.*)" />
            <conditions>
              <add input="{HTTPS}" pattern="off" ignoreCase="true" />
            </conditions>
            <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
          </rule>
        </rules>
      </rewrite>
    </system.webServer>
  </location>
</configuration>
```

#### **Step 6: Configure Application Settings**

Create `C:\inetpub\wwwroot\ScanPetAPI\appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-db-server;Port=5432;Database=scanpet_db;Username=scanpet_user;Password=YourPassword;"
  },
  "JwtSettings": {
    "PrivateKey": "YOUR_BASE64_PRIVATE_KEY",
    "PublicKey": "YOUR_BASE64_PUBLIC_KEY",
    "Issuer": "MobileBackendAPI",
    "Audience": "MobileApp"
  },
  "NLogSettings": {
    "LogDirectory": "C:\\inetpub\\wwwroot\\ScanPetAPI\\Logs\\",
    "ArchiveDirectory": "C:\\inetpub\\wwwroot\\ScanPetAPI\\Logs\\Archive\\"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://scanpet.com",
      "https://www.scanpet.com"
    ]
  }
}
```

#### **Step 7: Set Permissions**

```powershell
# Grant permissions to application pool identity
$acl = Get-Acl "C:\inetpub\wwwroot\ScanPetAPI"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\ScanPetAPI", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\wwwroot\ScanPetAPI" $acl

# Create logs directory
New-Item -Path "C:\inetpub\wwwroot\ScanPetAPI\Logs" -ItemType Directory -Force
New-Item -Path "C:\inetpub\wwwroot\ScanPetAPI\Logs\Archive" -ItemType Directory -Force
```

#### **Step 8: Start Website**

```powershell
# Start website
Start-Website -Name "ScanPetAPI"

# Restart application pool
Restart-WebAppPool -Name "ScanPetAPI"

# Test
Invoke-WebRequest -Uri "http://localhost/health"
```

---

### **.NET 9 on Linux with Nginx**

#### **Step 1: Install .NET 9 Runtime**

**Ubuntu/Debian:**
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET 9 Runtime
sudo apt update
sudo apt install aspnetcore-runtime-9.0 -y

# Verify installation
dotnet --list-runtimes
```

**CentOS/RHEL:**
```bash
# Add Microsoft repository
sudo rpm -Uvh https://packages.microsoft.com/config/centos/8/packages-microsoft-prod.rpm

# Install .NET 9 Runtime
sudo dnf install aspnetcore-runtime-9.0 -y
```

#### **Step 2: Publish & Deploy Application**

```bash
# On development machine, publish application
cd /path/to/ScanPet/src/API/MobileBackend.API
dotnet publish -c Release -o ./publish

# Copy to server (replace with your server IP)
scp -r ./publish/* user@your-server:/var/www/scanpet-api/

# Or on server, create directory
sudo mkdir -p /var/www/scanpet-api
sudo chown -R $USER:$USER /var/www/scanpet-api
```

#### **Step 3: Create Kestrel Service Configuration**

Create `appsettings.Production.json` on server:

```bash
sudo nano /var/www/scanpet-api/appsettings.Production.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=scanpet_db;Username=scanpet_user;Password=YourPassword;"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  },
  "JwtSettings": {
    "PrivateKey": "YOUR_BASE64_PRIVATE_KEY",
    "PublicKey": "YOUR_BASE64_PUBLIC_KEY"
  },
  "NLogSettings": {
    "LogDirectory": "/var/www/scanpet-api/logs/",
    "ArchiveDirectory": "/var/www/scanpet-api/logs/archive/"
  }
}
```

#### **Step 4: Create Systemd Service**

```bash
sudo nano /etc/systemd/system/scanpet-api.service
```

```ini
[Unit]
Description=ScanPet API - ASP.NET Core Application
After=network.target
After=postgresql.service

[Service]
Type=notify
WorkingDirectory=/var/www/scanpet-api
ExecStart=/usr/bin/dotnet /var/www/scanpet-api/MobileBackend.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=scanpet-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

# Security settings
PrivateTmp=true
NoNewPrivileges=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/www/scanpet-api/logs

[Install]
WantedBy=multi-user.target
```

**Enable and Start Service:**

```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service (start on boot)
sudo systemctl enable scanpet-api

# Start service
sudo systemctl start scanpet-api

# Check status
sudo systemctl status scanpet-api

# View logs
sudo journalctl -u scanpet-api -f
```

#### **Step 5: Install and Configure Nginx**

```bash
# Install Nginx
sudo apt install nginx -y

# Create Nginx configuration
sudo nano /etc/nginx/sites-available/scanpet-api
```

**Nginx Configuration:**

```nginx
# Upstream to Kestrel
upstream scanpet_backend {
    server localhost:5000;
    keepalive 32;
}

# Rate limiting
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/m;
limit_conn_zone $binary_remote_addr zone=addr:10m;

# HTTP Server (redirect to HTTPS)
server {
    listen 80;
    listen [::]:80;
    server_name api.scanpet.com;

    # Redirect to HTTPS
    return 301 https://$server_name$request_uri;
}

# HTTPS Server
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name api.scanpet.com;

    # SSL Configuration
    ssl_certificate /etc/ssl/certs/scanpet-api.crt;
    ssl_certificate_key /etc/ssl/private/scanpet-api.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # Security headers
    add_header X-Content-Type-Options nosniff always;
    add_header X-Frame-Options DENY always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer" always;
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    # Logging
    access_log /var/log/nginx/scanpet-api-access.log;
    error_log /var/log/nginx/scanpet-api-error.log warn;

    # Client body size limit
    client_max_body_size 10M;

    # Gzip compression
    gzip on;
    gzip_types application/json application/javascript text/css text/plain;
    gzip_min_length 1000;

    location / {
        # Rate limiting
        limit_req zone=api_limit burst=20 nodelay;
        limit_conn addr 10;

        # Proxy to Kestrel
        proxy_pass http://scanpet_backend;
        proxy_http_version 1.1;
        
        # Headers
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $server_name;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        # Buffering
        proxy_buffering on;
        proxy_cache_bypass $http_upgrade;
    }

    # Health check endpoint (no rate limit)
    location /health {
        proxy_pass http://scanpet_backend;
        access_log off;
    }

    # Deny access to sensitive files
    location ~ /\. {
        deny all;
        access_log off;
        log_not_found off;
    }
}
```

**Enable Site:**

```bash
# Create symbolic link
sudo ln -s /etc/nginx/sites-available/scanpet-api /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx

# Enable Nginx on boot
sudo systemctl enable nginx
```

#### **Step 6: Firewall Configuration**

```bash
# Allow HTTP and HTTPS
sudo ufw allow 'Nginx Full'

# Or specific ports
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Check status
sudo ufw status
```

---

### **.NET 9 with Docker**

#### **Complete docker-compose.yml**

Create `docker-compose.yml` in project root:

```yaml
version: '3.8'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:16-alpine
    container_name: scanpet-postgres
    restart: always
    environment:
      POSTGRES_DB: scanpet_db
      POSTGRES_USER: scanpet_user
      POSTGRES_PASSWORD: ${DB_PASSWORD:-YourStrongPassword123!}
      PGDATA: /var/lib/postgresql/data/pgdata
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - scanpet-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U scanpet_user -d scanpet_db"]
      interval: 10s
      timeout: 5s
      retries: 5

  # .NET 9 API
  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: scanpet-api
    restart: always
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=scanpet_db;Username=scanpet_user;Password=${DB_PASSWORD:-YourStrongPassword123!}
      - JwtSettings__PrivateKey=${JWT_PRIVATE_KEY}
      - JwtSettings__PublicKey=${JWT_PUBLIC_KEY}
      - JwtSettings__Issuer=MobileBackendAPI
      - JwtSettings__Audience=MobileApp
      - NLogSettings__LogDirectory=/var/data/logs/
      - NLogSettings__LogLevel=Info
    ports:
      - "8080:8080"
    volumes:
      - api-logs:/var/data/logs
      - api-uploads:/var/data/uploads
    networks:
      - scanpet-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  # Nginx Reverse Proxy
  nginx:
    image: nginx:alpine
    container_name: scanpet-nginx
    restart: always
    depends_on:
      - api
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
      - nginx-logs:/var/log/nginx
    networks:
      - scanpet-network

volumes:
  postgres-data:
    driver: local
  api-logs:
    driver: local
  api-uploads:
    driver: local
  nginx-logs:
    driver: local

networks:
  scanpet-network:
    driver: bridge
```

#### **Nginx Configuration for Docker**

Create `nginx.conf`:

```nginx
events {
    worker_connections 1024;
}

http {
    upstream api_backend {
        server api:8080;
    }

    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/m;

    server {
        listen 80;
        server_name localhost;

        location / {
            limit_req zone=api_limit burst=20 nodelay;
            
            proxy_pass http://api_backend;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection keep-alive;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            
            proxy_cache_bypass $http_upgrade;
        }
    }
}
```

#### **Deploy with Docker Compose**

```bash
# Create .env file
cat > .env << EOF
DB_PASSWORD=YourStrongPassword123!
JWT_PRIVATE_KEY=YOUR_BASE64_PRIVATE_KEY
JWT_PUBLIC_KEY=YOUR_BASE64_PUBLIC_KEY
EOF

# Build and start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# Check status
docker-compose ps

# Run migrations
docker-compose exec api dotnet ef database update

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

---

### **.NET 9 as Windows Service**

#### **Step 1: Publish as Self-Contained**

```powershell
cd C:\Users\malbujoq\source\repos\ScanPet\src\API\MobileBackend.API

# Publish as self-contained
dotnet publish -c Release -r win-x64 --self-contained true -o C:\Services\ScanPetAPI
```

#### **Step 2: Install as Windows Service**

```powershell
# Install using sc.exe
sc.exe create ScanPetAPI `
    binPath="C:\Services\ScanPetAPI\MobileBackend.API.exe" `
    DisplayName="ScanPet API Service" `
    start=auto

# Or use New-Service
New-Service -Name "ScanPetAPI" `
    -BinaryPathName "C:\Services\ScanPetAPI\MobileBackend.API.exe" `
    -DisplayName "ScanPet API Service" `
    -Description "ScanPet Mobile Backend API Service" `
    -StartupType Automatic

# Start service
Start-Service ScanPetAPI

# Check status
Get-Service ScanPetAPI

# View logs
Get-EventLog -LogName Application -Source ScanPetAPI -Newest 50
```

#### **Step 3: Configure Application**

Edit `C:\Services\ScanPetAPI\appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      },
      "Https": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "C:\\Services\\ScanPetAPI\\certificate.pfx",
          "Password": "YOUR_CERT_PASSWORD"
        }
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=scanpet_db;Username=scanpet_user;Password=YourPassword;"
  }
}
```

---

### **.NET 9 as Linux Systemd Service**

Already covered in **Step 4** of ".NET 9 on Linux with Nginx" section above.

**Quick Commands:**

```bash
# Start
sudo systemctl start scanpet-api

# Stop
sudo systemctl stop scanpet-api

# Restart
sudo systemctl restart scanpet-api

# Status
sudo systemctl status scanpet-api

# Enable on boot
sudo systemctl enable scanpet-api

# Disable
sudo systemctl disable scanpet-api

# View logs
sudo journalctl -u scanpet-api -f
sudo journalctl -u scanpet-api --since today
sudo journalctl -u scanpet-api --since "1 hour ago"
```

---

## ?? **Security Configuration**

### **1. SSL/TLS Certificate**

#### **Option A: Let's Encrypt (Free)**

**On Linux with Nginx:**

```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx -y

# Obtain certificate
sudo certbot --nginx -d api.scanpet.com

# Auto-renewal (test)
sudo certbot renew --dry-run

# Certificate files will be at:
# /etc/letsencrypt/live/api.scanpet.com/fullchain.pem
# /etc/letsencrypt/live/api.scanpet.com/privkey.pem
```

#### **Option B: Self-Signed Certificate (Testing)**

**Windows:**

```powershell
# Create self-signed certificate
$cert = New-SelfSignedCertificate `
    -DnsName "api.scanpet.local" `
    -CertStoreLocation "cert:\LocalMachine\My" `
    -NotAfter (Get-Date).AddYears(5)

# Export certificate
$pwd = ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "C:\scanpet-api.pfx" -Password $pwd

# Bind to IIS site
Import-Module WebAdministration
$binding = Get-WebBinding -Name "ScanPetAPI" -Protocol https
$binding.AddSslCertificate($cert.Thumbprint, "my")
```

**Linux:**

```bash
# Generate self-signed certificate
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /etc/ssl/private/scanpet-api.key \
    -out /etc/ssl/certs/scanpet-api.crt \
    -subj "/C=US/ST=State/L=City/O=Organization/CN=api.scanpet.local"

# Set permissions
sudo chmod 600 /etc/ssl/private/scanpet-api.key
```

### **2. Firewall Rules**

**Windows Firewall:**

```powershell
# Allow HTTP
New-NetFirewallRule -DisplayName "HTTP" -Direction Inbound -LocalPort 80 -Protocol TCP -Action Allow

# Allow HTTPS
New-NetFirewallRule -DisplayName "HTTPS" -Direction Inbound -LocalPort 443 -Protocol TCP -Action Allow

# Block all except specific IPs (optional)
New-NetFirewallRule -DisplayName "API Access" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow -RemoteAddress 192.168.1.0/24
```

**Linux UFW:**

```bash
# Allow SSH (don't lock yourself out!)
sudo ufw allow 22/tcp

# Allow HTTP/HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow from specific IP only (optional)
sudo ufw allow from 192.168.1.0/24 to any port 5000

# Enable firewall
sudo ufw enable

# Check status
sudo ufw status numbered
```

### **3. Security Hardening**

**File Permissions (Linux):**

```bash
# Set correct ownership
sudo chown -R www-data:www-data /var/www/scanpet-api

# Set correct permissions
sudo find /var/www/scanpet-api -type d -exec chmod 755 {} \;
sudo find /var/www/scanpet-api -type f -exec chmod 644 {} \;

# Make executable
sudo chmod +x /var/www/scanpet-api/MobileBackend.API.dll
```

**Disable Unnecessary Services:**

```bash
# List running services
sudo systemctl list-units --type=service --state=running

# Disable unused services
sudo systemctl disable bluetooth
sudo systemctl disable cups
```

---

## ?? **Monitoring & Maintenance**

### **1. Log Monitoring**

**Windows:**

```powershell
# View IIS logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC1\*.log" -Tail 50 -Wait

# View Application logs
Get-Content "C:\inetpub\wwwroot\ScanPetAPI\Logs\*.log" -Tail 50 -Wait

# Event Viewer
Get-EventLog -LogName Application -Source "IIS*" -Newest 50
```

**Linux:**

```bash
# View Nginx logs
sudo tail -f /var/log/nginx/scanpet-api-access.log
sudo tail -f /var/log/nginx/scanpet-api-error.log

# View application logs
sudo tail -f /var/www/scanpet-api/logs/*.log

# View systemd logs
sudo journalctl -u scanpet-api -f
```

### **2. Performance Monitoring**

**Windows:**

```powershell
# CPU and Memory usage
Get-Process -Name "w3wp" | Select-Object CPU, WorkingSet64

# Application Pool status
Get-WebAppPoolState -Name "ScanPetAPI"
```

**Linux:**

```bash
# Monitor resources
htop

# Check application
sudo systemctl status scanpet-api

# Check Nginx
sudo systemctl status nginx

# Check database connections
sudo -u postgres psql -c "SELECT count(*) FROM pg_stat_activity WHERE datname='scanpet_db';"
```

### **3. Automated Backups**

**Database Backup (Windows):**

```powershell
# Create backup script: C:\Scripts\backup-db.ps1
$date = Get-Date -Format "yyyy-MM-dd-HHmm"
$backupFile = "C:\Backups\scanpet_db_$date.backup"
& "C:\Program Files\PostgreSQL\16\bin\pg_dump.exe" -U postgres -Fc scanpet_db > $backupFile

# Remove backups older than 30 days
Get-ChildItem "C:\Backups" -Filter "*.backup" | Where-Object { $_.CreationTime -lt (Get-Date).AddDays(-30) } | Remove-Item

# Schedule with Task Scheduler
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\Scripts\backup-db.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "ScanPet DB Backup" -Description "Daily database backup"
```

**Database Backup (Linux):**

```bash
# Create backup script: /usr/local/bin/backup-db.sh
#!/bin/bash
DATE=$(date +%Y-%m-%d-%H%M)
BACKUP_DIR="/backups/database"
mkdir -p $BACKUP_DIR

# Backup
sudo -u postgres pg_dump -Fc scanpet_db > "$BACKUP_DIR/scanpet_db_$DATE.backup"

# Remove old backups (keep 30 days)
find $BACKUP_DIR -name "*.backup" -mtime +30 -delete

# Make executable
chmod +x /usr/local/bin/backup-db.sh

# Add to crontab (2 AM daily)
(crontab -l 2>/dev/null; echo "0 2 * * * /usr/local/bin/backup-db.sh") | crontab -
```

---

## ?? **Troubleshooting**

### **Common Issues**

#### **1. Application Won't Start**

**Windows IIS:**

```powershell
# Check Application Pool status
Get-WebAppPoolState -Name "ScanPetAPI"

# Check Event Viewer
Get-EventLog -LogName Application -Source "IIS*" -Newest 20

# Check stdout logs
Get-Content "C:\inetpub\wwwroot\ScanPetAPI\logs\stdout*.log"

# Restart app pool
Restart-WebAppPool -Name "ScanPetAPI"
```

**Linux:**

```bash
# Check service status
sudo systemctl status scanpet-api

# View detailed logs
sudo journalctl -u scanpet-api -n 100 --no-pager

# Check for port conflicts
sudo netstat -tulpn | grep :5000

# Restart service
sudo systemctl restart scanpet-api
```

#### **2. Database Connection Failed**

```bash
# Test connection
psql -h localhost -U scanpet_user -d scanpet_db

# Check PostgreSQL is running
sudo systemctl status postgresql

# Check connections
sudo -u postgres psql -c "SELECT * FROM pg_stat_activity WHERE datname='scanpet_db';"

# Check pg_hba.conf
sudo cat /etc/postgresql/16/main/pg_hba.conf
```

#### **3. 502 Bad Gateway (Nginx)**

```bash
# Check if app is running
curl http://localhost:5000/health

# Check Nginx error log
sudo tail -f /var/log/nginx/scanpet-api-error.log

# Check upstream connection
sudo netstat -tulpn | grep :5000

# Test Nginx config
sudo nginx -t

# Restart services
sudo systemctl restart scanpet-api
sudo systemctl restart nginx
```

#### **4. High Memory Usage**

```bash
# Check memory
free -h

# Check app memory
ps aux | grep dotnet

# Restart application
sudo systemctl restart scanpet-api

# Check for memory leaks in logs
sudo journalctl -u scanpet-api | grep -i "memory\|exception"
```

#### **5. Slow Performance**

**Database:**

```sql
-- Check slow queries
SELECT * FROM pg_stat_statements ORDER BY total_time DESC LIMIT 10;

-- Check active connections
SELECT count(*) FROM pg_stat_activity WHERE state = 'active';

-- Analyze tables
ANALYZE;

-- Vacuum database
VACUUM ANALYZE;
```

**Application:**

```bash
# Check response time
curl -o /dev/null -s -w "Time: %{time_total}s\n" http://localhost:5000/health

# Check logs for slow queries
sudo journalctl -u scanpet-api | grep -i "slow\|timeout"
```

---

## ?? **Additional Resources**

### **Official Documentation:**
- [ASP.NET Core Deployment](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [IIS Hosting](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)
- [Linux Hosting](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Nginx Documentation](https://nginx.org/en/docs/)

### **Security:**
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [SSL Labs](https://www.ssllabs.com/ssltest/) - Test your SSL configuration
- [Security Headers](https://securityheaders.com/) - Check security headers

---

**Last Updated:** January 15, 2025  
**Guide Version:** 1.0  
**Target Stack:** .NET 9, PostgreSQL 16, Windows Server 2019+, Ubuntu 20.04+

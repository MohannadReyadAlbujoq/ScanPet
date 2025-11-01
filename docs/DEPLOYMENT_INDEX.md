# ?? Complete Deployment Documentation Index - ScanPet Mobile Backend

## ?? **Quick Navigation**

This document provides the complete header structure for all deployment guides, organized by deployment type (Online Cloud vs On-Premises Server).

---

## ?? **Documentation Structure**

```
docs/
??? DEPLOYMENT_GUIDE_FREE_HOSTING.md          ? Online Cloud Deployment
??? DEPLOYMENT_ON_PREMISES_SERVER.md          ? On-Premises Server Deployment
??? APP_SETTINGS_COMPLETE_GUIDE.md            ? Configuration Reference
??? DEPLOYMENT_QUICK_REFERENCE.md             ? Quick Commands
??? DEPLOYMENT_SUMMARY.md                     ? Overview
```

---

## ?? **Online Cloud Deployment** 
**?? File:** `docs/DEPLOYMENT_GUIDE_FREE_HOSTING.md`

### **Header Navigation:**

```
1. Overview
   ?? Why Free Hosting?
   ?? What You'll Deploy

2. Free Hosting Options
   ?? 2.1 Database Providers
   ?   ?? Neon.tech (Recommended)
   ?   ?? Supabase
   ?   ?? ElephantSQL
   ?   ?? Render PostgreSQL
   ?? 2.2 Backend API Providers
   ?   ?? Render (Recommended)
   ?   ?? Railway
   ?   ?? Fly.io
   ?   ?? Azure App Service
   ?? 2.3 Frontend Providers
       ?? Vercel (Recommended)
       ?? Netlify
       ?? GitHub Pages
       ?? Cloudflare Pages

3. Database Deployment (Online)
   ?? 3.1 Neon.tech Setup
   ?   ?? Create Account & Project
   ?   ?? Get Connection Details
   ?   ?? Run Database Migrations
   ?? 3.2 Supabase Setup (Alternative)
   ?   ?? Create Project
   ?   ?? Get Connection String
   ?   ?? Configure Authentication
   ?? 3.3 Connection String Format
       ?? PostgreSQL Format
       ?? Environment Variables
       ?? Security Best Practices

4. Backend API Deployment (Online)
   ?? 4.1 Render Deployment
   ?   ?? Prepare Repository (Dockerfile)
   ?   ?? Create Web Service
   ?   ?? Configure Environment Variables
   ?   ?? Deploy Application
   ?   ?? Run Migrations
   ?? 4.2 Railway Deployment (Alternative)
   ?   ?? Connect GitHub
   ?   ?? Configure Service
   ?   ?? Add Environment Variables
   ?? 4.3 Fly.io Deployment (Alternative)
       ?? Install Fly CLI
       ?? Launch Application
       ?? Deploy

5. Frontend Deployment (Online)
   ?? 5.1 Vercel Setup
   ?   ?? Prepare Frontend (API URL)
   ?   ?? Import Project
   ?   ?? Configure Environment Variables
   ?   ?? Deploy
   ?? 5.2 Netlify (Alternative)
   ?? 5.3 Update Backend CORS

6. Configuration Files
   ?? 6.1 Dockerfile
   ?? 6.2 .dockerignore
   ?? 6.3 appsettings.Production.json
   ?? 6.4 .env.production (Frontend)
   ?? 6.5 Program.cs Updates

7. Troubleshooting (Online)
   ?? 7.1 Database Connection Failed
   ?? 7.2 Render Build Failed
   ?? 7.3 API Returns 500 Error
   ?? 7.4 CORS Error
   ?? 7.5 Service Sleeps (Free Tier)

8. Monitoring & Maintenance
   ?? 8.1 Check Logs (Render/Vercel)
   ?? 8.2 Monitor Database (Neon)
   ?? 8.3 Update Code (Auto-Deploy)
   ?? 8.4 Keep Service Awake (UptimeRobot)

9. Cost Breakdown
   ?? Free vs Paid Tiers

10. Deployment Checklist
    ?? Pre-Deployment
    ?? Database Setup
    ?? Backend Deployment
    ?? Frontend Deployment
    ?? Testing
```

---

## ??? **On-Premises Server Deployment**
**?? File:** `docs/DEPLOYMENT_ON_PREMISES_SERVER.md`

### **Header Navigation:**

```
1. Overview
   ?? Why On-Premises?
   ?? Requirements

2. Database Deployment - On-Premises
   ?? 2.1 PostgreSQL on Windows Server
   ?   ?? Step 1: Install PostgreSQL
   ?   ?   ?? Download & Install
   ?   ?   ?? Installation Wizard
   ?   ?? Step 2: Configure PostgreSQL
   ?   ?   ?? Edit postgresql.conf
   ?   ?   ?? Edit pg_hba.conf
   ?   ?   ?? Restart Service
   ?   ?? Step 3: Create Database & User
   ?   ?   ?? Open pgAdmin/psql
   ?   ?   ?? CREATE DATABASE
   ?   ?   ?? CREATE USER
   ?   ?   ?? GRANT PRIVILEGES
   ?   ?? Step 4: Run Migrations
   ?   ?   ?? dotnet ef database update
   ?   ?? Step 5: Windows Firewall
   ?       ?? New-NetFirewallRule
   ?? 2.2 PostgreSQL on Linux Server
   ?   ?? Step 1: Install PostgreSQL (Ubuntu/Debian)
   ?   ?   ?? apt install postgresql-16
   ?   ?? Step 2: Configure PostgreSQL
   ?   ?   ?? Edit postgresql.conf
   ?   ?   ?? Edit pg_hba.conf
   ?   ?   ?? systemctl restart
   ?   ?? Step 3: Create Database & User
   ?   ?   ?? CREATE DATABASE / CREATE USER
   ?   ?? Step 4: Firewall (UFW)
   ?       ?? ufw allow 5432/tcp
   ?? 2.3 PostgreSQL with Docker
       ?? Create docker-compose.yml
       ?? Start Container
       ?? Connection String

3. Backend API Deployment - On-Premises
   ?? 3.1 .NET 9 on Windows Server with IIS
   ?   ?? Step 1: Install Prerequisites
   ?   ?   ?? Install .NET 9 Hosting Bundle
   ?   ?   ?? Install IIS
   ?   ?   ?? Install URL Rewrite Module
   ?   ?? Step 2: Publish Application
   ?   ?   ?? dotnet publish
   ?   ?? Step 3: Create Application Pool
   ?   ?   ?? New-WebAppPool
   ?   ?? Step 4: Create IIS Website
   ?   ?   ?? Using PowerShell
   ?   ?   ?? Using IIS Manager
   ?   ?? Step 5: Configure web.config
   ?   ?   ?? AspNetCore Module
   ?   ?   ?? Security Headers
   ?   ?   ?? Compression
   ?   ?   ?? URL Rewrite (HTTP?HTTPS)
   ?   ?? Step 6: Configure appsettings.Production.json
   ?   ?   ?? Connection Strings, Paths
   ?   ?? Step 7: Set Permissions
   ?   ?   ?? Grant to Application Pool Identity
   ?   ?? Step 8: Start Website
   ?       ?? Start-Website
   ?       ?? Test
   ?? 3.2 .NET 9 on Linux with Nginx
   ?   ?? Step 1: Install .NET 9 Runtime
   ?   ?   ?? Ubuntu/Debian
   ?   ?   ?? CentOS/RHEL
   ?   ?? Step 2: Publish & Deploy
   ?   ?   ?? dotnet publish
   ?   ?   ?? Copy to /var/www/scanpet-api
   ?   ?? Step 3: Create Kestrel Configuration
   ?   ?   ?? appsettings.Production.json
   ?   ?? Step 4: Create Systemd Service
   ?   ?   ?? /etc/systemd/system/scanpet-api.service
   ?   ?   ?? systemctl enable
   ?   ?   ?? systemctl start
   ?   ?? Step 5: Install Nginx
   ?   ?   ?? apt install nginx
   ?   ?   ?? Create /etc/nginx/sites-available/scanpet-api
   ?   ?? Step 6: Configure Nginx
   ?   ?   ?? Upstream Configuration
   ?   ?   ?? HTTP Server (Redirect)
   ?   ?   ?? HTTPS Server
   ?   ?   ?? SSL Configuration
   ?   ?   ?? Security Headers
   ?   ?   ?? Rate Limiting
   ?   ?   ?? Proxy Settings
   ?   ?   ?? Enable Site
   ?   ?? Step 7: Firewall Configuration
   ?       ?? ufw allow 80/443
   ?? 3.3 .NET 9 with Docker
   ?   ?? Complete docker-compose.yml
   ?   ?   ?? PostgreSQL Service
   ?   ?   ?? API Service
   ?   ?   ?? Nginx Service
   ?   ?? Nginx Configuration
   ?   ?? Deploy with Docker Compose
   ?       ?? Create .env
   ?       ?? docker-compose up
   ?       ?? Run Migrations
   ?? 3.4 .NET 9 as Windows Service
   ?   ?? Step 1: Publish as Self-Contained
   ?   ?? Step 2: Install as Service
   ?   ?   ?? sc.exe create
   ?   ?   ?? New-Service
   ?   ?? Step 3: Configure & Start
   ?? 3.5 .NET 9 as Linux Systemd Service
       ?? Create Service File
       ?? Enable & Start
       ?? Management Commands

4. Security Configuration
   ?? 4.1 SSL/TLS Certificate Setup
   ?   ?? Option A: Let's Encrypt (Free)
   ?   ?   ?? certbot --nginx
   ?   ?? Option B: Self-Signed (Testing)
   ?       ?? Windows: New-SelfSignedCertificate
   ?       ?? Linux: openssl req
   ?? 4.2 Firewall Rules
   ?   ?? Windows Firewall
   ?   ?   ?? New-NetFirewallRule
   ?   ?? Linux UFW
   ?       ?? ufw allow
   ?? 4.3 Security Hardening
       ?? File Permissions
       ?? Disable Unnecessary Services
       ?? SELinux / AppArmor

5. Monitoring & Maintenance
   ?? 5.1 Log Monitoring
   ?   ?? Windows
   ?   ?   ?? IIS Logs
   ?   ?   ?? Application Logs
   ?   ?   ?? Event Viewer
   ?   ?? Linux
   ?       ?? Nginx Logs
   ?       ?? Application Logs
   ?       ?? Systemd Logs
   ?? 5.2 Performance Monitoring
   ?   ?? Windows
   ?   ?   ?? Get-Process
   ?   ?? Linux
   ?       ?? htop
   ?       ?? systemctl status
   ?? 5.3 Automated Backups
       ?? Database Backup (Windows)
       ?   ?? Create Script
       ?   ?? Schedule with Task Scheduler
       ?? Database Backup (Linux)
           ?? Create Script
           ?? Add to Crontab

6. Troubleshooting
   ?? 6.1 Application Won't Start
   ?   ?? Windows IIS
   ?   ?? Linux Systemd
   ?? 6.2 Database Connection Failed
   ?? 6.3 502 Bad Gateway (Nginx)
   ?? 6.4 High Memory Usage
   ?? 6.5 Slow Performance
       ?? Database Queries
       ?? Application Performance

7. Additional Resources
   ?? Official Documentation Links
```

---

## ?? **App Settings Configuration Guide**
**?? File:** `docs/APP_SETTINGS_COMPLETE_GUIDE.md`

### **Header Navigation:**

```
1. Overview
   ?? Configuration Files

2. Configuration Hierarchy
   ?? Priority Order

3. Connection Strings
   ?? 3.1 Settings Explained
   ?? 3.2 PostgreSQL Format
   ?   ?? Components
   ?   ?? Additional Options
   ?? 3.3 SQL Server Format
   ?? 3.4 Environment-Specific Examples
   ?   ?? Development
   ?   ?? Production
   ?   ?? Environment Variables
   ?? 3.5 Security Best Practices

4. JWT Settings
   ?? 4.1 Settings Explained
   ?? 4.2 Generate JWT Keys
   ?   ?? PowerShell
   ?   ?? Bash
   ?? 4.3 Token Expiry Guidelines
   ?? 4.4 Security Best Practices

5. Security Settings
   ?? 5.1 Settings Explained
   ?? 5.2 Environment-Specific Examples
   ?   ?? Development
   ?   ?? Production
   ?   ?? High Security
   ?? 5.3 Password Complexity

6. NLog Settings
   ?? 6.1 Settings Explained
   ?? 6.2 Log Levels
   ?? 6.3 Environment-Specific Examples
   ?   ?? Development
   ?   ?? Production (Windows)
   ?   ?? Production (Linux)
   ?   ?? Production (Docker)
   ?? 6.4 Disk Space Calculations

7. CORS Configuration
   ?? 7.1 Settings Explained
   ?? 7.2 Origin Patterns
   ?? 7.3 Security Best Practices
   ?? 7.4 Environment-Specific

8. API Settings
   ?? 8.1 Settings Explained
   ?? 8.2 Rate Limiting Guidelines
   ?? 8.3 Swagger Security

9. Environment-Specific Configuration
   ?? 9.1 Development Example
   ?? 9.2 Production Example

10. Secrets Management
    ?? 10.1 Environment Variables
    ?   ?? Linux/Mac
    ?   ?? Windows
    ?   ?? Docker
    ?   ?? Docker Compose
    ?? 10.2 User Secrets (Development)
    ?? 10.3 Azure Key Vault
    ?? 10.4 AWS Secrets Manager

11. Best Practices
    ?? 11.1 Never Commit Secrets
    ?? 11.2 Different Keys Per Environment
    ?? 11.3 Rotate Keys Regularly
    ?? 11.4 Validate Configuration
    ?? 11.5 Separate Databases

12. Complete Examples
    ?? 12.1 Development (Local)
    ?? 12.2 Production (On-Premises)
    ?? 12.3 Production (Cloud - Render/Railway)
    ?? 12.4 Docker Deployment

13. Configuration Validation
    ?? Startup Validation Code

14. Configuration Troubleshooting
    ?? Connection String Not Found
    ?? JWT Keys Not Working
    ?? CORS Still Blocked

15. Configuration Checklist
    ?? Before Deployment
```

---

## ?? **Quick Reference**
**?? File:** `docs/DEPLOYMENT_QUICK_REFERENCE.md`

### **Header Navigation:**

```
1. Free Hosting Options
   ?? Database Providers
   ?? Backend API Providers
   ?? Frontend Providers

2. Quick Start Deployment
   ?? Step 1: Database (5 min)
   ?? Step 2: Backend API (10 min)
   ?? Step 3: Frontend (5 min)

3. Configuration Files
   ?? .dockerignore
   ?? .env.production

4. Database Setup
   ?? Neon.tech Connection String
   ?? Run Migrations

5. URLs After Deployment

6. Troubleshooting
   ?? Database Connection Failed
   ?? Render Build Failed
   ?? CORS Error
   ?? API Returns 500
   ?? Render Service Sleeps

7. Monitoring

8. Cost Breakdown

9. Deployment Checklist

10. Quick Commands
```

---

## ?? **Deployment Summary**
**?? File:** `docs/DEPLOYMENT_SUMMARY.md`

### **Header Navigation:**

```
1. Status & Overview

2. Recommended Free Hosting Stack

3. What's Prepared
   ?? Configuration Files
   ?? Documentation
   ?? Code Updates

4. Deployment Steps
   ?? Step 1: Database
   ?? Step 2: Generate JWT Keys
   ?? Step 3: Create .dockerignore
   ?? Step 4: Deploy Backend
   ?? Step 5: Run Migrations
   ?? Step 6: Deploy Frontend
   ?? Step 7: Test

5. Your Deployed URLs

6. Configuration Summary
   ?? Database
   ?? Backend
   ?? Frontend

7. Common Issues & Solutions

8. Monitoring & Maintenance

9. Cost & Limits

10. Next Steps
```

---

## ?? **Usage Guide**

### **For Online Cloud Deployment:**

1. **Start Here:** `DEPLOYMENT_GUIDE_FREE_HOSTING.md`
   - Complete step-by-step for Neon.tech + Render + Vercel
   - Free hosting, no server management

2. **Quick Commands:** `DEPLOYMENT_QUICK_REFERENCE.md`
   - Copy-paste commands
   - Environment variable templates

3. **Configuration:** `APP_SETTINGS_COMPLETE_GUIDE.md`
   - Section 9: Environment-Specific Configuration
   - Section 10: Secrets Management
   - Section 12.3: Production (Cloud) Example

### **For On-Premises Server Deployment:**

1. **Start Here:** `DEPLOYMENT_ON_PREMISES_SERVER.md`
   - Complete guide for Windows Server or Linux Server
   - Full control, no monthly costs

2. **Choose Your Path:**
   - **Windows:** Section 2.1 (PostgreSQL) + Section 3.1 (IIS)
   - **Linux:** Section 2.2 (PostgreSQL) + Section 3.2 (Nginx)
   - **Docker:** Section 2.3 (PostgreSQL) + Section 3.3 (Docker)

3. **Configuration:** `APP_SETTINGS_COMPLETE_GUIDE.md`
   - Section 3: Connection Strings
   - Section 9: Environment-Specific Configuration
   - Section 12.2: Production (On-Premises) Example

4. **Security:** `DEPLOYMENT_ON_PREMISES_SERVER.md`
   - Section 4: Security Configuration
   - SSL/TLS certificates
   - Firewall rules

---

## ?? **Complete File Mapping**

| Topic | Primary File | Secondary Files |
|-------|--------------|-----------------|
| **Online Cloud Deployment** | `DEPLOYMENT_GUIDE_FREE_HOSTING.md` | `DEPLOYMENT_QUICK_REFERENCE.md`, `DEPLOYMENT_SUMMARY.md` |
| **On-Premises Deployment** | `DEPLOYMENT_ON_PREMISES_SERVER.md` | `APP_SETTINGS_COMPLETE_GUIDE.md` |
| **App Configuration** | `APP_SETTINGS_COMPLETE_GUIDE.md` | All deployment guides |
| **Quick Reference** | `DEPLOYMENT_QUICK_REFERENCE.md` | `DEPLOYMENT_SUMMARY.md` |
| **Overview** | `DEPLOYMENT_SUMMARY.md` | All guides |

---

## ?? **Search by Topic**

### **Database Setup:**
- **Online:** `DEPLOYMENT_GUIDE_FREE_HOSTING.md` ? Section 3
- **On-Premises Windows:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 2.1
- **On-Premises Linux:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 2.2
- **Docker:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 2.3

### **Backend API Deployment:**
- **Online (Render):** `DEPLOYMENT_GUIDE_FREE_HOSTING.md` ? Section 4.1
- **Windows IIS:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 3.1
- **Linux Nginx:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 3.2
- **Docker:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 3.3

### **Configuration:**
- **All Settings:** `APP_SETTINGS_COMPLETE_GUIDE.md` ? Sections 3-8
- **Secrets Management:** `APP_SETTINGS_COMPLETE_GUIDE.md` ? Section 10
- **Environment Variables:** `APP_SETTINGS_COMPLETE_GUIDE.md` ? Section 10.1
- **Examples:** `APP_SETTINGS_COMPLETE_GUIDE.md` ? Section 12

### **Security:**
- **SSL/TLS:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 4.1
- **Firewall:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 4.2
- **JWT Keys:** `APP_SETTINGS_COMPLETE_GUIDE.md` ? Section 4.2
- **CORS:** `APP_SETTINGS_COMPLETE_GUIDE.md` ? Section 7

### **Troubleshooting:**
- **Online:** `DEPLOYMENT_GUIDE_FREE_HOSTING.md` ? Section 7
- **On-Premises:** `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 6
- **Configuration:** `APP_SETTINGS_COMPLETE_GUIDE.md` ? Section 14

---

## ? **Complete Coverage Matrix**

| Question | Answer Location |
|----------|-----------------|
| **How to deploy database online?** | `DEPLOYMENT_GUIDE_FREE_HOSTING.md` ? Section 3 |
| **How to deploy database on server?** | `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 2 |
| **How to configure app settings?** | `APP_SETTINGS_COMPLETE_GUIDE.md` ? All sections |
| **How to build and deploy backend online?** | `DEPLOYMENT_GUIDE_FREE_HOSTING.md` ? Section 4 |
| **How to build and deploy backend on server?** | `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 3 |
| **Free hosting options?** | `DEPLOYMENT_GUIDE_FREE_HOSTING.md` ? Section 2 |
| **Environment-specific config?** | `APP_SETTINGS_COMPLETE_GUIDE.md` ? Section 9 |
| **Secrets management?** | `APP_SETTINGS_COMPLETE_GUIDE.md` ? Section 10 |
| **SSL/TLS setup?** | `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Section 4.1 |
| **Docker deployment?** | `DEPLOYMENT_ON_PREMISES_SERVER.md` ? Sections 2.3, 3.3 |

---

**Last Updated:** January 15, 2025  
**Total Documentation:** 5 comprehensive guides  
**Total Content:** 10,000+ lines  
**Status:** ? Complete

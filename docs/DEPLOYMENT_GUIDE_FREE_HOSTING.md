# ?? Complete Free Deployment Guide - ScanPet Mobile Backend

## ?? **Overview**

This guide provides step-by-step instructions to deploy your ScanPet Mobile Backend API and frontend using **100% FREE** hosting services.

**Tech Stack:**
- **Backend**: .NET 9 Web API
- **Database**: PostgreSQL
- **Frontend**: React/Next.js (or any SPA)

---

## ?? **Recommended Free Hosting Combo**

| Component | Provider | Free Tier | Why? |
|-----------|----------|-----------|------|
| **Database** | [Neon.tech](https://neon.tech) | 0.5 GB, 1 compute hour | Best PostgreSQL free tier |
| **Backend API** | [Render](https://render.com) | 750 hours/month | .NET 9 support, auto-deploy |
| **Frontend** | [Vercel](https://vercel.com) | 100 GB bandwidth | React/Next.js optimized |

---

## ?? **Prerequisites**

- ? GitHub account
- ? Your ScanPet code pushed to GitHub
- ? Email account for sign-ups
- ? Basic command line knowledge

---

## ??? **STEP 1: Database Deployment (Neon.tech)**

### **1.1 Create Account & Project**

1. **Go to**: https://neon.tech
2. **Sign Up** with GitHub (easiest)
3. **Create New Project**:
   ```
   Project Name: scanpet
   Region: [Choose closest to your users]
   PostgreSQL Version: 16 (latest)
   ```

### **1.2 Get Connection Details**

After project creation, you'll see a connection string like:
```
postgresql://scanpet_owner:npg_AbCd1234EfGh@ep-cool-forest-12345678.us-east-2.aws.neon.tech/scanpet?sslmode=require
```

**Parse this into:**
```
Host: ep-cool-forest-12345678.us-east-2.aws.neon.tech
Database: scanpet
Username: scanpet_owner
Password: npg_AbCd1234EfGh
Port: 5432
SSL Mode: Require
```

**?? Copy this connection string - you'll need it!**

### **1.3 Run Database Migrations (Local)**

Before deploying, initialize your database:

```powershell
# Update connection string in appsettings.json temporarily
cd C:\Users\malbujoq\source\repos\ScanPet\src\API\MobileBackend.API

# Run migrations
dotnet ef database update --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj
```

**Verify Success:**
```powershell
# Check tables created
dotnet ef migrations list --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj
```

---

## ??? **STEP 2: Backend API Deployment (Render)**

### **2.1 Prepare Your Repository**

#### **Create `Dockerfile` in root**

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["src/API/MobileBackend.API/MobileBackend.API.csproj", "src/API/MobileBackend.API/"]
COPY ["src/Application/MobileBackend.Application/MobileBackend.Application.csproj", "src/Application/MobileBackend.Application/"]
COPY ["src/Infrastructure/MobileBackend.Infrastructure/MobileBackend.Infrastructure.csproj", "src/Infrastructure/MobileBackend.Infrastructure/"]
COPY ["src/Domain/MobileBackend.Domain/MobileBackend.Domain.csproj", "src/Domain/MobileBackend.Domain/"]
COPY ["src/Framework/MobileBackend.Framework/MobileBackend.Framework.csproj", "src/Framework/MobileBackend.Framework/"]

# Restore dependencies
RUN dotnet restore "src/API/MobileBackend.API/MobileBackend.API.csproj"

# Copy everything
COPY . .

# Build
WORKDIR "/src/src/API/MobileBackend.API"
RUN dotnet build "MobileBackend.API.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "MobileBackend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Create log directories
RUN mkdir -p /var/data/logs /var/data/logs/archive /var/data/uploads

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MobileBackend.API.dll"]
```

#### **Create `.dockerignore` in root**

```
**/bin
**/obj
**/out
**/.vs
**/.vscode
**/node_modules
**/dist
**/build
**/*.log
**/.git
```

#### **Update `Program.cs` for Docker**

Ensure your `Program.cs` listens on correct port:

```csharp
// At the top of Program.cs
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
```

#### **Commit and Push**

```powershell
git add .
git commit -m "Add Docker support for deployment"
git push origin main
```

### **2.2 Deploy to Render**

1. **Sign Up**: https://render.com (Use GitHub)

2. **Create New Web Service**:
   - Click **"New +"** ? **"Web Service"**
   - **Connect Repository**: Choose `ScanPet`
   - Click **"Connect"**

3. **Configure Service**:
   ```
   Name: scanpet-api
   Region: Oregon (US West) or closest
   Branch: main
   Runtime: Docker
   Plan: Free
   ```

4. **Advanced Settings**:
   ```
   Health Check Path: /health
   Auto-Deploy: Yes
   ```

### **2.3 Add Environment Variables**

Click **"Environment"** tab and add:

#### **Required Variables:**

```bash
# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
PORT=8080

# Database (from Neon.tech)
ConnectionStrings__DefaultConnection=postgresql://scanpet_owner:npg_AbCd1234EfGh@ep-cool-forest-12345678.us-east-2.aws.neon.tech/scanpet?sslmode=require

# JWT Settings (Generate these!)
JwtSettings__PrivateKey=YOUR_BASE64_RSA_PRIVATE_KEY
JwtSettings__PublicKey=YOUR_BASE64_RSA_PUBLIC_KEY
JwtSettings__Issuer=MobileBackendAPI
JwtSettings__Audience=MobileApp
JwtSettings__AccessTokenExpiryMinutes=15
JwtSettings__RefreshTokenExpiryDays=7

# Logging
NLogSettings__LogDirectory=/var/data/logs/
NLogSettings__ArchiveDirectory=/var/data/logs/archive/
NLogSettings__LogLevel=Info
NLogSettings__EnableHtmlLogging=false
```

#### **Generate JWT Keys (PowerShell)**

```powershell
# Generate RSA keys
$rsa = [System.Security.Cryptography.RSA]::Create(2048)

# Export private key
$privateKey = [Convert]::ToBase64String($rsa.ExportRSAPrivateKey())
Write-Host "Private Key: $privateKey"

# Export public key
$publicKey = [Convert]::ToBase64String($rsa.ExportRSAPublicKey())
Write-Host "Public Key: $publicKey"
```

**Copy these keys to environment variables!**

### **2.4 Deploy**

1. Click **"Create Web Service"**
2. Render will:
   - Build Docker image
   - Run migrations (if configured)
   - Deploy application
   
3. **Get Your URL**: `https://scanpet-api.onrender.com`

4. **Test API**:
   ```
   https://scanpet-api.onrender.com/health
   https://scanpet-api.onrender.com/swagger (if enabled)
   ```

### **2.5 Run Migrations on Deployed Database**

Option 1: **Render Shell**
```bash
# In Render dashboard ? Shell
dotnet ef database update
```

Option 2: **Local with Production Connection**
```powershell
# Temporarily update appsettings.Production.json
dotnet ef database update --connection "YOUR_NEON_CONNECTION_STRING"
```

---

## ?? **STEP 3: Frontend Deployment (Vercel)**

### **3.1 Prepare Frontend**

#### **Update API Base URL**

Create `.env.production` in frontend root:

```bash
# API URL from Render
REACT_APP_API_URL=https://scanpet-api.onrender.com
NEXT_PUBLIC_API_URL=https://scanpet-api.onrender.com
VITE_API_URL=https://scanpet-api.onrender.com
```

#### **Update CORS in Backend**

Your backend should now allow frontend domain:

```json
// appsettings.Production.json (already updated)
"Cors": {
  "AllowedOrigins": [
    "https://scanpet.vercel.app",
    "https://*.vercel.app"
  ]
}
```

### **3.2 Deploy to Vercel**

1. **Sign Up**: https://vercel.com (Use GitHub)

2. **Import Project**:
   - Click **"Add New..."** ? **"Project"**
   - **Import Git Repository**
   - Select your frontend repository

3. **Configure Project**:
   ```
   Framework Preset: [Auto-detected - React/Next.js]
   Root Directory: ./ (or frontend folder if in monorepo)
   Build Command: npm run build
   Output Directory: dist (or build)
   ```

4. **Environment Variables**:
   ```
   REACT_APP_API_URL=https://scanpet-api.onrender.com
   ```

5. **Deploy**:
   - Click **"Deploy"**
   - Vercel builds and deploys
   - Get URL: `https://scanpet.vercel.app`

### **3.3 Test End-to-End**

1. Open: `https://scanpet.vercel.app`
2. Try login/register
3. Verify API calls work

---

## ?? **STEP 4: Configuration Files Summary**

### **Files to Update Before Deployment:**

#### **1. Dockerfile** (Root)
? Created above

#### **2. appsettings.Production.json** (API)
? Already updated

#### **3. .env.production** (Frontend)
```bash
REACT_APP_API_URL=https://scanpet-api.onrender.com
```

#### **4. Program.cs** (API)
Add port configuration:
```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
```

---

## ?? **STEP 5: Troubleshooting**

### **Common Issues:**

#### **1. Database Connection Failed**

**Error**: `Could not connect to database`

**Solutions**:
```bash
# Verify connection string format
postgresql://user:password@host:5432/database?sslmode=require

# Check Neon.tech project is active
# Check SSL mode is set to "Require"
```

#### **2. Render Build Failed**

**Error**: `Build failed with exit code 1`

**Solutions**:
```bash
# Check Dockerfile paths are correct
# Verify all .csproj files exist
# Check for missing NuGet packages
```

#### **3. API Returns 500 Error**

**Error**: `Internal Server Error`

**Solutions**:
```bash
# Check Render logs: Dashboard ? Logs
# Verify environment variables set
# Check database migrations ran
```

#### **4. CORS Error in Frontend**

**Error**: `Access-Control-Allow-Origin`

**Solutions**:
```csharp
// Update appsettings.Production.json
"Cors": {
  "AllowedOrigins": [
    "https://your-frontend.vercel.app",
    "https://*.vercel.app"
  ]
}

// Redeploy backend
```

#### **5. Render Service Sleeps**

**Issue**: Free tier sleeps after 15 min inactivity

**Solutions**:
```bash
# Use UptimeRobot (free) to ping every 14 minutes
https://uptimerobot.com

# Or accept 50-second wake-up time (free tier limitation)
```

---

## ?? **STEP 6: Monitoring & Maintenance**

### **6.1 Check Logs**

**Render Logs:**
```
Dashboard ? Your Service ? Logs (real-time)
```

**Vercel Logs:**
```
Dashboard ? Your Project ? Deployments ? View Logs
```

### **6.2 Monitor Database**

**Neon Dashboard:**
```
Dashboard ? Your Project ? Monitoring
- Active connections
- Storage usage
- Query performance
```

### **6.3 Update Code**

```bash
# Local changes
git add .
git commit -m "Update feature X"
git push origin main

# Auto-deploys on Render & Vercel!
```

---

## ?? **STEP 7: Cost Breakdown**

### **Current Setup (100% FREE):**

| Service | Cost | Limitations |
|---------|------|-------------|
| **Neon.tech** | $0 | 0.5 GB storage, 1 compute hour |
| **Render** | $0 | 750 hours/month, sleeps after 15 min |
| **Vercel** | $0 | 100 GB bandwidth, unlimited deployments |
| **TOTAL** | **$0/month** | Perfect for development/testing |

### **Upgrade Path (When Needed):**

| Service | Paid Plan | Cost | Benefits |
|---------|-----------|------|----------|
| **Neon** | Pro | $19/mo | 10 GB, always-on |
| **Render** | Starter | $7/mo | Always-on, no sleep |
| **Vercel** | Pro | $20/mo | 1 TB bandwidth, analytics |
| **TOTAL** | | **$46/mo** | Production-ready |

---

## ? **STEP 8: Deployment Checklist**

### **Pre-Deployment:**
- [ ] Code pushed to GitHub
- [ ] Dockerfile created
- [ ] Environment variables documented
- [ ] JWT keys generated
- [ ] Database migrations tested

### **Database:**
- [ ] Neon.tech account created
- [ ] PostgreSQL database created
- [ ] Connection string saved
- [ ] Migrations ran successfully
- [ ] Test data inserted

### **Backend:**
- [ ] Render account created
- [ ] Docker image builds locally
- [ ] Environment variables configured
- [ ] API deployed successfully
- [ ] Health check endpoint works
- [ ] Swagger accessible (if enabled)

### **Frontend:**
- [ ] Vercel account created
- [ ] API URL configured
- [ ] Environment variables set
- [ ] Build succeeds
- [ ] Deployment successful
- [ ] Can connect to API

### **Testing:**
- [ ] Registration works
- [ ] Login works
- [ ] API calls successful
- [ ] CORS configured correctly
- [ ] Error handling works
- [ ] Performance acceptable

---

## ?? **STEP 9: Quick Reference**

### **URLs:**

```bash
# Database
https://console.neon.tech/

# Backend API
https://scanpet-api.onrender.com

# Backend Swagger (if enabled)
https://scanpet-api.onrender.com/swagger

# Frontend
https://scanpet.vercel.app

# Logs & Monitoring
https://dashboard.render.com/
https://vercel.com/dashboard
```

### **Connection Strings:**

```bash
# Neon PostgreSQL
postgresql://user:password@host.neon.tech:5432/database?sslmode=require

# Format for appsettings
ConnectionStrings__DefaultConnection=postgresql://...
```

### **Environment Variables Template:**

```bash
# Save this for reference
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
PORT=8080

ConnectionStrings__DefaultConnection=postgresql://[YOUR_NEON_CONNECTION]

JwtSettings__PrivateKey=[YOUR_BASE64_PRIVATE_KEY]
JwtSettings__PublicKey=[YOUR_BASE64_PUBLIC_KEY]
JwtSettings__Issuer=MobileBackendAPI
JwtSettings__Audience=MobileApp

NLogSettings__LogDirectory=/var/data/logs/
NLogSettings__LogLevel=Info
```

---

## ?? **Additional Resources**

### **Official Documentation:**

- [Neon.tech Docs](https://neon.tech/docs)
- [Render Docs](https://render.com/docs)
- [Vercel Docs](https://vercel.com/docs)
- [.NET 9 Deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/)

### **Monitoring Tools (Free):**

- [UptimeRobot](https://uptimerobot.com) - Keep Render awake
- [StatusCake](https://www.statuscake.com) - Uptime monitoring
- [LogRocket](https://logrocket.com) - Frontend error tracking

### **Alternatives:**

| Need | Alternative | Why |
|------|-------------|-----|
| Database | Supabase | Includes auth & storage |
| Backend | Railway | $5 free credit |
| Backend | Fly.io | 3 shared VMs free |
| Frontend | Netlify | Similar to Vercel |

---

## ?? **Next Steps**

### **After Successful Deployment:**

1. **Set Up Monitoring**
   - Add UptimeRobot to keep API awake
   - Configure error alerts

2. **Configure Custom Domain** (Optional)
   ```
   Vercel: Settings ? Domains
   Render: Settings ? Custom Domain
   ```

3. **Enable HTTPS** (Automatic on Render/Vercel)

4. **Set Up CI/CD** (Already done via GitHub!)
   - Push to GitHub ? Auto-deploy

5. **Add Tests**
   - Configure GitHub Actions
   - Run tests before deploy

6. **Monitor Performance**
   - Check Render logs
   - Monitor database queries
   - Track API response times

---

## ?? **Support**

### **Common Commands:**

```powershell
# Build locally
dotnet build

# Run migrations
dotnet ef database update

# Test API
dotnet run

# Build Docker image locally
docker build -t scanpet-api .
docker run -p 8080:8080 scanpet-api

# Generate JWT keys
# (See Step 2.3 above)
```

### **If You Get Stuck:**

1. **Check Logs**:
   - Render: Dashboard ? Logs
   - Vercel: Deployment ? Function Logs

2. **Verify Environment Variables**:
   - Render: Settings ? Environment
   - Vercel: Settings ? Environment Variables

3. **Test Locally First**:
   ```powershell
   # Set environment variable
   $env:ConnectionStrings__DefaultConnection="YOUR_NEON_CONNECTION"
   
   # Run API
   dotnet run
   ```

4. **Community Help**:
   - [Render Community](https://community.render.com)
   - [Vercel Discussions](https://github.com/vercel/vercel/discussions)
   - [Neon Discord](https://discord.gg/neon)

---

## ? **Success Checklist**

You're done when:

- ? Database is accessible
- ? Backend API returns 200 on `/health`
- ? Swagger UI loads (if enabled)
- ? Frontend loads without errors
- ? Can register new user
- ? Can login
- ? Can make authenticated API calls
- ? CORS works correctly
- ? No errors in logs

---

**?? Congratulations! Your ScanPet application is now deployed and accessible worldwide! ??**

**URLs to Share:**
- API: `https://scanpet-api.onrender.com`
- Frontend: `https://scanpet.vercel.app`

---

**Last Updated:** January 15, 2025  
**Guide Version:** 1.0  
**Target Stack:** .NET 9, PostgreSQL, React

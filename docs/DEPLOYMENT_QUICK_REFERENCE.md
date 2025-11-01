# ?? Deployment Quick Reference - ScanPet

## ? **FREE HOSTING OPTIONS**

### **Database (Choose ONE):**

| Provider | URL | Free Tier | Best For |
|----------|-----|-----------|----------|
| **Neon.tech** ? | https://neon.tech | 0.5 GB, PostgreSQL 16 | **RECOMMENDED** |
| **Supabase** | https://supabase.com | 500 MB + Auth + Storage | Feature-rich |
| **ElephantSQL** | https://elephantsql.com | 20 MB | Minimal |
| **Render DB** | https://render.com | 90 days free | Temporary testing |

### **Backend API (Choose ONE):**

| Provider | URL | Free Tier | Best For |
|----------|-----|-----------|----------|
| **Render** ? | https://render.com | 750 hrs/mo, Docker | **RECOMMENDED** |
| **Railway** | https://railway.app | $5 free credit | Easy setup |
| **Fly.io** | https://fly.io | 3 VMs, 256MB RAM | Global deployment |
| **Azure** | https://azure.com | F1 tier | Microsoft ecosystem |

### **Frontend (Choose ONE):**

| Provider | URL | Free Tier | Best For |
|----------|-----|-----------|----------|
| **Vercel** ? | https://vercel.com | 100 GB bandwidth | **React/Next.js** |
| **Netlify** | https://netlify.com | 100 GB bandwidth | Static sites |
| **Cloudflare** | https://pages.cloudflare.com | Unlimited | Fast CDN |
| **GitHub Pages** | https://pages.github.com | 1 GB storage | Public repos |

---

## ?? **QUICK START DEPLOYMENT**

### **Step 1: Database (5 minutes)**

```bash
1. Go to: https://neon.tech
2. Sign up with GitHub
3. Create project: "scanpet"
4. Copy connection string:
   postgresql://user:pass@host.neon.tech:5432/db?sslmode=require
5. Save this string! ??
```

### **Step 2: Backend API (10 minutes)**

```bash
1. Go to: https://render.com
2. Sign up with GitHub
3. New ? Web Service
4. Connect repository: "ScanPet"
5. Runtime: Docker
6. Add environment variables (see below)
7. Deploy! ??
```

**Environment Variables for Render:**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
PORT=8080

ConnectionStrings__DefaultConnection=[YOUR_NEON_CONNECTION_STRING]

JwtSettings__PrivateKey=[GENERATE_THIS]
JwtSettings__PublicKey=[GENERATE_THIS]
JwtSettings__Issuer=MobileBackendAPI
JwtSettings__Audience=MobileApp

NLogSettings__LogDirectory=/var/data/logs/
NLogSettings__LogLevel=Info
```

**Generate JWT Keys (PowerShell):**
```powershell
$rsa = [System.Security.Cryptography.RSA]::Create(2048)
$private = [Convert]::ToBase64String($rsa.ExportRSAPrivateKey())
$public = [Convert]::ToBase64String($rsa.ExportRSAPublicKey())
Write-Host "Private: $private"
Write-Host "Public: $public"
```

### **Step 3: Frontend (5 minutes)**

```bash
1. Go to: https://vercel.com
2. Sign up with GitHub
3. Import project ? Select frontend repo
4. Add environment variable:
   REACT_APP_API_URL=https://your-api.onrender.com
5. Deploy! ??
```

---

## ?? **CONFIGURATION FILES**

### **Files Already Updated:**

? `appsettings.json` - Development settings  
? `appsettings.Production.json` - Production settings  
? `Dockerfile` - Already exists in your repo  

### **Files to Create:**

#### **1. `.dockerignore` (Root)**
```
**/bin
**/obj
**/out
**/.vs
**/.vscode
**/node_modules
**/*.log
**/.git
**/dist
**/build
```

#### **2. `.env.production` (Frontend)**
```bash
REACT_APP_API_URL=https://your-api-name.onrender.com
# or for Next.js:
NEXT_PUBLIC_API_URL=https://your-api-name.onrender.com
# or for Vite:
VITE_API_URL=https://your-api-name.onrender.com
```

---

## ??? **DATABASE SETUP**

### **Neon.tech Connection String Format:**

```
postgresql://[USERNAME]:[PASSWORD]@[HOST]:[PORT]/[DATABASE]?sslmode=require
```

**Example:**
```
postgresql://scanpet_owner:AbCd1234@ep-cool-123.us-east-2.aws.neon.tech:5432/scanpet?sslmode=require
```

### **Run Migrations:**

**Option 1: Local (Before Deployment)**
```powershell
cd C:\Users\malbujoq\source\repos\ScanPet\src\API\MobileBackend.API

# Update connection string in appsettings.json temporarily
dotnet ef database update --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj
```

**Option 2: After Deployment (Render Shell)**
```bash
# In Render dashboard ? Shell
dotnet ef database update
```

---

## ?? **URLS AFTER DEPLOYMENT**

After successful deployment, you'll have:

```
Database:     https://console.neon.tech/
Backend API:  https://scanpet-api.onrender.com
Swagger:      https://scanpet-api.onrender.com/swagger (if enabled)
Frontend:     https://scanpet.vercel.app
```

---

## ?? **TROUBLESHOOTING**

### **Issue 1: Database Connection Failed**

```bash
# Verify connection string format
postgresql://user:password@host:5432/database?sslmode=require

# Check SSL mode is included: ?sslmode=require
# Check password doesn't contain special characters that need encoding
```

### **Issue 2: Render Build Failed**

```bash
# Check Dockerfile exists in root
# Verify all paths in Dockerfile are correct
# Check logs in Render dashboard
```

### **Issue 3: CORS Error**

```json
// Update appsettings.Production.json
"Cors": {
  "AllowedOrigins": [
    "https://your-frontend.vercel.app",
    "https://*.vercel.app"
  ]
}
```

### **Issue 4: API Returns 500**

```bash
# Check Render logs: Dashboard ? Logs
# Verify all environment variables are set
# Check database migrations ran successfully
```

### **Issue 5: Render Service Sleeps**

```bash
# Free tier sleeps after 15 min inactivity
# Use UptimeRobot (free) to ping every 14 minutes
# Or accept 50-second wake time
```

---

## ?? **MONITORING**

### **Check Logs:**

```bash
# Render (Backend)
Dashboard ? Your Service ? Logs

# Vercel (Frontend)
Dashboard ? Deployments ? View Logs

# Neon (Database)
Dashboard ? Monitoring
```

### **Keep API Awake (Optional):**

```bash
# Sign up: https://uptimerobot.com
# Add monitor:
  URL: https://your-api.onrender.com/health
  Interval: 14 minutes
```

---

## ?? **COST BREAKDOWN**

### **100% FREE Setup:**

| Service | Cost | Limits |
|---------|------|--------|
| Neon | $0 | 0.5 GB database |
| Render | $0 | 750 hours/month (enough for 1 app) |
| Vercel | $0 | 100 GB bandwidth |
| **TOTAL** | **$0/month** | ? Perfect for development |

### **Upgrade When Needed:**

| Service | Plan | Cost | Benefits |
|---------|------|------|----------|
| Neon | Pro | $19/mo | 10 GB, always-on |
| Render | Starter | $7/mo | No sleep, custom domain |
| Vercel | Pro | $20/mo | 1 TB bandwidth |
| **TOTAL** | | **$46/mo** | Production-ready |

---

## ? **DEPLOYMENT CHECKLIST**

### **Before Deployment:**
- [ ] Code pushed to GitHub
- [ ] Dockerfile exists in root
- [ ] .dockerignore created
- [ ] JWT keys generated
- [ ] Connection strings ready

### **Database:**
- [ ] Neon account created
- [ ] Database created
- [ ] Connection string copied
- [ ] Migrations tested locally

### **Backend:**
- [ ] Render account created
- [ ] Repository connected
- [ ] Environment variables configured
- [ ] Service deployed
- [ ] Health endpoint works

### **Frontend:**
- [ ] Vercel account created
- [ ] Repository connected
- [ ] Environment variables set
- [ ] Build successful
- [ ] Site accessible

### **Testing:**
- [ ] Can register new user
- [ ] Can login
- [ ] API calls work
- [ ] CORS configured
- [ ] No errors in logs

---

## ?? **QUICK COMMANDS**

### **Local Testing:**

```powershell
# Build
dotnet build

# Run API
cd src\API\MobileBackend.API
dotnet run

# Test API
curl http://localhost:5000/health

# Run migrations
dotnet ef database update
```

### **Docker Testing:**

```powershell
# Build image
docker build -t scanpet-api .

# Run container
docker run -p 8080:8080 -e "ConnectionStrings__DefaultConnection=YOUR_CONNECTION" scanpet-api

# Test
curl http://localhost:8080/health
```

---

## ?? **NEXT STEPS**

After successful deployment:

1. **Custom Domain** (Optional)
   - Vercel: Settings ? Domains
   - Render: Settings ? Custom Domain

2. **Monitoring**
   - Set up UptimeRobot
   - Configure error alerts

3. **CI/CD** (Auto-configured!)
   - Push to GitHub ? Auto-deploy ?

4. **SSL/HTTPS** (Automatic!)
   - Both Render and Vercel provide free SSL ?

5. **Scaling** (When needed)
   - Upgrade to paid plans
   - Add Redis for caching
   - Use CDN for static files

---

## ?? **SUPPORT RESOURCES**

### **Official Docs:**
- [Neon Docs](https://neon.tech/docs)
- [Render Docs](https://render.com/docs)
- [Vercel Docs](https://vercel.com/docs)

### **Community:**
- [Render Community](https://community.render.com)
- [Vercel Discussions](https://github.com/vercel/vercel/discussions)
- [Neon Discord](https://discord.gg/neon)

### **Monitoring Tools:**
- [UptimeRobot](https://uptimerobot.com) - Keep API awake
- [StatusCake](https://statuscake.com) - Uptime monitoring
- [Sentry](https://sentry.io) - Error tracking (free tier)

---

## ?? **SUCCESS!**

Once deployed, share your URLs:

```
?? Frontend: https://scanpet.vercel.app
?? API: https://scanpet-api.onrender.com
?? Swagger: https://scanpet-api.onrender.com/swagger
```

---

**Complete Guide:** `docs/DEPLOYMENT_GUIDE_FREE_HOSTING.md`  
**Last Updated:** January 15, 2025  
**Status:** ? Production-Ready

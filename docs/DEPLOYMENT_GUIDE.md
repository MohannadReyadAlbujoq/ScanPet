# ?? FREE DEPLOYMENT GUIDE - ScanPet Backend API

## ? **Best Free Hosting: Railway.app**

### **Quick Deployment:**

1. **Go to:** https://railway.app
2. **Sign up** with GitHub
3. **New Project** ? **Deploy from GitHub**
4. **Add PostgreSQL Database**
5. **Set Environment Variables:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=${POSTGRES_URL}
   ```
6. **Deploy!**

**Your API URL:** `https://scanpet-api.up.railway.app`

---

## ?? **Build Commands:**

```powershell
# Build for deployment
dotnet publish src/API/MobileBackend.API/MobileBackend.API.csproj -c Release -o ./publish
```

---

## ?? **Other Free Options:**

| Platform | Free Tier | Database | URL |
|----------|-----------|----------|-----|
| **Railway.app** | $5/month credits | PostgreSQL included | https://railway.app |
| **Render.com** | 750 hours/month | PostgreSQL 90 days | https://render.com |
| **Fly.io** | 3 shared VMs | PostgreSQL included | https://fly.io |

**Recommended:** Railway.app (easiest + best free tier)

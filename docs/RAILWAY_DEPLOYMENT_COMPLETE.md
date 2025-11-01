# ?? RAILWAY DEPLOYMENT - COMPLETE SETUP GUIDE

## ? **Step-by-Step Deployment**

### **1. Push Your Code to GitHub**

```powershell
# Navigate to your project
cd C:\Users\malbujoq\source\repos\ScanPet

# Initialize git (if needed)
git init
git add .
git commit -m "Initial commit - Production ready"

# Push to GitHub
git remote add origin https://github.com/YOUR_USERNAME/ScanPet.git
git branch -M main
git push -u origin main
```

---

### **2. Create Railway Project**

1. Go to **https://railway.app**
2. Click **"+ New"** button
3. Select **"Deploy from GitHub repo"**
4. Choose your **ScanPet** repository
5. Railway will analyze and deploy automatically

---

### **3. Add PostgreSQL Database**

1. In your Railway project, click **"+ New"**
2. Select **"Database"** ? **"Add PostgreSQL"**
3. Railway creates database and provides `DATABASE_URL`

---

### **4. Configure Environment Variables**

**In Railway Project Settings ? Variables, add these:**

#### **Required Variables:**

```bash
# Environment
ASPNETCORE_ENVIRONMENT=Production

# Database (Auto-filled by Railway PostgreSQL)
DATABASE_URL=${POSTGRES_URL}

# JWT Keys (COPY FROM BELOW)
JWT_PRIVATE_KEY=PFJTQUtleVZhbHVlPjxNb2R1bHVzPjNJU3FOT2xBZ1hUYld6TGtpT0NhQXRGK2NOVGFIMFhtQmRxWUhmeFBOaDlWMDhZeGRSWEROMWF5L0lkUUhNZldLOFphbXRVaVJjRmVKNlZaZW1RbEZ2UEE4cWllWEZlNi9MS1o0VW9SUVVDaXVPeitZMHJFWFNjN2d0MUM3L0JEMDczNDBDR0d4RGprSlhlUyt1NkJMRGhMTGpMMU9UZlNIU1R3cVdvQkl0VVFpcXIzd3Q1cVZyUEE4dEQ0bDV5d2JneXdJYytSa0l3UUJiVDdQUVRjV24wblRZZnBLb2YvSSt5Qk5yZWNodk9EV0Y5eWxKUEJvQk15Z1pjUFhTMElINHI1WlMwSWtubFRwb0dvQndjbFRaSlZtR0JSQXdOMEo2TlgzTFRoWGU0K1RPN1JsdmhLcVl1TWdRd2Q2c3cwbHpMWXdKcTdjZERHVU5KV0ZmbGJLUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48UD44T2YrMHludkhSc0pRR2JsVjdQZ0QxZ0paN1U2ZHVqbEJwV3VNaWhuTGY1ZWxxTERFUm51R3praDl2UVd0MkRpY3hkd1h6RDZYeWcwT3lGa1VIWUtoY2o5WXF0L1cxT2VFTmVtdUlhT3pVakFURnZudDY3Ym1JUTJtdFFzUGNzLzY3QmpWSHFxWmt1Y1E3elhqRzVXNkoyeTRHZ2RGcVBuaVVBaDhMcEFKSHM9PC9QPjxRPjZsV29Lb3ZDSE5MN2NrWWNZMGVrcmEwTXRtbXoyNTU3eWk2WUZzMjMyZGtjZUNqQjNvQm5wdjBneFRDZWFYS0FXbW9GcmFWTWMxdnUvRnlBbC9tbG9zQzd1VFg1UDR4Q2VBZFVQejBBVmhmUW05NnU3T3dteFF1OUdGUGFKeVlJemhLc0RuU0V2OFRDZVNscXpGbVI1dk4xNzllK2M1SjBPSnR3UzBYQzU2cz08L1E+PERQPml3U2x4YitPTDlLWlBTUGp0ditqRGRlMDNiYjBQUWhhbWJrb28rTXkzNVRKaXMzMEdWdElUMGRoOVR1WUhFeDVnUWNHbnJnSjAzM0UzbFovcC9ybVNadlp0T1hZZ2FBNE4rbnFXTk8xZG50RUZReDRKRVJ3am92Rll3V0xYa0Y2Nm95SVZZalA2bVk1ajlGYTVid0t4UU96NU5IUDcyKzRQampYSFM2dHN4OD08L0RQPjxEUT5wVVNtSnVPTWtxR21YekRkWXBPcnJDV3BHcG13Rk10UWlRYVZremVoVzg1SXl1SUw0UzMwYkl5SDZTcmgvb0tYemF5S3RxNG5IaEVQbXdKdVcwRGh0Y2h5WFN5WXpsM1FuekNrRlRtam5CU050dkMxdERqVkd4R3RXcFFBL3ExUGRtOHEwOE1Qd09RM09CYWkwTjYzRFFoVi9FTFBlbGtuQ0tsdjZGYWFJS009PC9EUT48SW52ZXJzZVE+V3hmbW4rNGhpN3A5N25PSGlFNHNiM0xXQ2NMYkxNTU41RnBQSmgwSGwwaWFSL1JHU3FWdTJKU20zTnNZZnEwSElvNWp6TThXV3hrcTIzTkhzZzl5WXdaUEIxRmdpQ01BSFFOMmd4aWVxaFl2eE8wbWJlVzk3WE1obzhGS2R2WjU5V2JtN0lBTU9tTlMzaXNtUWhmVG5CQytPbkdpdkNYRkk4Mis0aG5RdVRVPTwvSW52ZXJzZVE+PEQ+ZW53dDFtbmc3SWN0K0VBU2RIemN6c2Z5UlZYVk9oMm5JSVZFalJOQ0pEdlBmbXZOdU0wM3U1c3FtOTNFLzlkMy9LU1dXdjRicFVjOHR5Uyt5SjZyZzA0dFZCdVlxbzRJVEtnZmxPV0JKQTkwMHViQWZnSmZnNlp4QmNWRWt1V1BweU1UK3ZkRVZlV0x3OHZoRmdRNE40bWxLS29oVDREVDhtQThWTDJPbC90YzFjUnB0YnZ0OFplYWcrZ1N2b3NISnJ3ZWFmOFc5azdST2RUMHJUQ25rNFVzZTdxUFNDcS9jUHNsZlZWbnhGdVN4U2FxM3NMNDVUbXM0V2FvL0t5cHZCdnZ3R0lOdEcvdDRsbkx1ZzRFYzlLaEhzRVFSRXltY2VhYnc4aFBVRUczbHFWR1gyZjRMaVNYdzlCaThYZ0EreFBEbVUrWlZ2OHVWTG9nUXlhbXlRPT08L0Q+PC9SU0FLZXlWYWx1ZT4=

JWT_PUBLIC_KEY=PFJTQUtleVZhbHVlPjxNb2R1bHVzPjNJU3FOT2xBZ1hUYld6TGtpT0NhQXRGK2NOVGFIMFhtQmRxWUhmeFBOaDlWMDhZeGRSWEROMWF5L0lkUUhNZldLOFphbXRVaVJjRmVKNlZaZW1RbEZ2UEE4cWllWEZlNi9MS1o0VW9SUVVDaXVPeitZMHJFWFNjN2d0MUM3L0JEMDczNDBDR0d4RGprSlhlUyt1NkJMRGhMTGpMMU9UZlNIU1R3cVdvQkl0VVFpcXIzd3Q1cVZyUEE4dEQ0bDV5d2JneXdJYytSa0l3UUJiVDdQUVRjV24wblRZZnBLb2YvSSt5Qk5yZWNodk9EV0Y5eWxKUEJvQk15Z1pjUFhTMElINHI1WlMwSWtubFRwb0dvQndjbFRaSlZtR0JSQXdOMEo2TlgzTFRoWGU0K1RPN1JsdmhLcVl1TWdRd2Q2c3cwbHpMWXdKcTdjZERHVU5KV0ZmbGJLUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg==

# JWT Settings
JwtSettings__Issuer=MobileBackendAPI
JwtSettings__Audience=MobileApp
JwtSettings__AccessTokenExpiryMinutes=15
JwtSettings__RefreshTokenExpiryDays=7
```

---

### **5. Update appsettings.Production.json to Use Env Vars**

Update to read from environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${DATABASE_URL}"
  },
  "JwtSettings": {
    "PrivateKey": "${JWT_PRIVATE_KEY}",
    "PublicKey": "${JWT_PUBLIC_KEY}",
    "Issuer": "${JwtSettings__Issuer}",
    "Audience": "${JwtSettings__Audience}",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "NLogSettings": {
    "EnableHtmlLogging": false,
    "LogDirectory": "/app/logs/",
    "MaxFileSize": 5000000
  }
}
```

---

### **6. Update Program.cs for Railway DATABASE_URL**

Railway provides `DATABASE_URL` in format:
```
postgresql://user:pass@host:5432/dbname
```

Add this to `Program.cs` before database configuration:

```csharp
// Convert Railway DATABASE_URL to proper format
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
}

// Also read JWT keys from environment
var jwtPrivateKey = Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY");
var jwtPublicKey = Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY");
if (!string.IsNullOrEmpty(jwtPrivateKey))
{
    builder.Configuration["JwtSettings:PrivateKey"] = jwtPrivateKey;
    builder.Configuration["JwtSettings:PublicKey"] = jwtPublicKey;
}
```

---

### **7. Deploy!**

Railway automatically:
1. Detects .NET 8 project
2. Runs `dotnet restore`
3. Runs `dotnet build`
4. Runs `dotnet publish`
5. Starts your API
6. Applies migrations
7. Seeds database

---

### **8. Test Your Deployment**

Once deployed, Railway gives you a URL like:
```
https://scanpet-production.up.railway.app
```

**Test endpoints:**

```bash
# Health check
curl https://your-app.up.railway.app/health

# Login
curl -X POST https://your-app.up.railway.app/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

---

## ?? **Railway Environment Variables Summary**

**Copy these exact values to Railway:**

| Variable | Value |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `DATABASE_URL` | *(Auto-filled by Railway PostgreSQL)* |
| `JWT_PRIVATE_KEY` | `PFJTQUtleVZhbHVlPjxNb2R1bHVzPjNJU3FOT2xBZ1hUYld6TGtpT0NhQXRGK2NOVGFIMFhtQmRxWUhmeFBOaDlWMDhZeGRSWEROMWF5L0lkUUhNZldLOFphbXRVaVJjRmVKNlZaZW1RbEZ2UEE4cWllWEZlNi9MS1o0VW9SUVVDaXVPeitZMHJFWFNjN2d0MUM3L0JEMDczNDBDR0d4RGprSlhlUyt1NkJMRGhMTGpMMU9UZlNIU1R3cVdvQkl0VVFpcXIzd3Q1cVZyUEE4dEQ0bDV5d2JneXdJYytSa0l3UUJiVDdQUVRjV24wblRZZnBLb2YvSSt5Qk5yZWNodk9EV0Y5eWxKUEJvQk15Z1pjUFhTMElINHI1WlMwSWtubFRwb0dvQndjbFRaSlZtR0JSQXdOMEo2TlgzTFRoWGU0K1RPN1JsdmhLcVl1TWdRd2Q2c3cwbHpMWXdKcTdjZERHVU5KV0ZmbGJLUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48UD44T2YrMHludkhSc0pRR2JsVjdQZ0QxZ0paN1U2ZHVqbEJwV3VNaWhuTGY1ZWxxTERFUm51R3praDl2UVd0MkRpY3hkd1h6RDZYeWcwT3lGa1VIWUtoY2o5WXF0L1cxT2VFTmVtdUlhT3pVakFURnZudDY3Ym1JUTJtdFFzUGNzLzY3QmpWSHFxWmt1Y1E3elhqRzVXNkoyeTRHZ2RGcVBuaVVBaDhMcEFKSHM9PC9QPjxRPjZsV29Lb3ZDSE5MN2NrWWNZMGVrcmEwTXRtbXoyNTU3eWk2WUZzMjMyZGtjZUNqQjNvQm5wdjBneFRDZWFYS0FXbW9GcmFWTWMxdnUvRnlBbC9tbG9zQzd1VFg1UDR4Q2VBZFVQejBBVmhmUW05NnU3T3dteFF1OUdGUGFKeVlJemhLc0RuU0V2OFRDZVNscXpGbVI1dk4xNzllK2M1SjBPSnR3UzBYQzU2cz08L1E+PERQPml3U2x4YitPTDlLWlBTUGp0ditqRGRlMDNiYjBQUWhhbWJrb28rTXkzNVRKaXMzMEdWdElUMGRoOVR1WUhFeDVnUWNHbnJnSjAzM0UzbFovcC9ybVNadlp0T1hZZ2FBNE4rbnFXTk8xZG50RUZReDRKRVJ3am92Rll3V0xYa0Y2Nm95SVZZalA2bVk1ajlGYTVid0t4UU96NU5IUDcyKzRQampYSFM2dHN4OD08L0RQPjxEUT5wVVNtSnVPTWtxR21YekRkWXBPcnJDV3BHcG13Rk10UWlRYVZremVoVzg1SXl1SUw0UzMwYkl5SDZTcmgvb0tYemF5S3RxNG5IaEVQbXdKdVcwRGh0Y2h5WFN5WXpsM1FuekNrRlRtam5CU050dkMxdERqVkd4R3RXcFFBL3ExUGRtOHEwOE1Qd09RM09CYWkwTjYzRFFoVi9FTFBlbGtuQ0tsdjZGYWFJS009PC9EUT48SW52ZXJzZVE+V3hmbW4rNGhpN3A5N25PSGlFNHNiM0xXQ2NMYkxNTU41RnBQSmgwSGwwaWFSL1JHU3FWdTJKU20zTnNZZnEwSElvNWp6TThXV3hrcTIzTkhzZzl5WXdaUEIxRmdpQ01BSFFOMmd4aWVxaFl2eE8wbWJlVzk3WE1obzhGS2R2WjU5V2JtN0lBTU9tTlMzaXNtUWhmVG5CQytPbkdpdkNYRkk4Mis0aG5RdVRVPTwvSW52ZXJzZVE+PEQ+ZW53dDFtbmc3SWN0K0VBU2RIemN6c2Z5UlZYVk9oMm5JSVZFalJOQ0pEdlBmbXZOdU0wM3U1c3FtOTNFLzlkMy9LU1dXdjRicFVjOHR5Uyt5SjZyZzA0dFZCdVlxbzRJVEtnZmxPV0JKQTkwMHViQWZnSmZnNlp4QmNWRWt1V1BweU1UK3ZkRVZlV0x3OHZoRmdRNE40bWxLS29oVDREVDhtQThWTDJPbC90YzFjUnB0YnZ0OFplYWcrZ1N2b3NISnJ3ZWFmOFc5azdST2RUMHJUQ25rNFVzZTdxUFNDcS9jUHNsZlZWbnhGdVN4U2FxM3NMNDVUbXM0V2FvL0t5cHZCdnZ3R0lOdEcvdDRsbkx1ZzRFYzlLaEhzRVFSRXltY2VhYnc4aFBVRUczbHFWR1gyZjRMaVNYdzlCaThYZ0EreFBEbVUrWlZ2OHVWTG9nUXlhbXlRPT08L0Q+PC9SU0FLZXlWYWx1ZT4=` |
| `JWT_PUBLIC_KEY` | `PFJTQUtleVZhbHVlPjxNb2R1bHVzPjNJU3FOT2xBZ1hUYld6TGtpT0NhQXRGK2NOVGFIMFhtQmRxWUhmeFBOaDlWMDhZeGRSWEROMWF5L0lkUUhNZldLOFphbXRVaVJjRmVKNlZaZW1RbEZ2UEE4cWllWEZlNi9MS1o0VW9SUVVDaXVPeitZMHJFWFNjN2d0MUM3L0JEMDczNDBDR0d4RGprSlhlUyt1NkJMRGhMTGpMMU9UZlNIU1R3cVdvQkl0VVFpcXIzd3Q1cVZyUEE4dEQ0bDV5d2JneXdJYytSa0l3UUJiVDdQUVRjV24wblRZZnBLb2YvSSt5Qk5yZWNodk9EV0Y5eWxKUEJvQk15Z1pjUFhTMElINHI1WlMwSWtubFRwb0dvQndjbFRaSlZtR0JSQXdOMEo2TlgzTFRoWGU0K1RPN1JsdmhLcVl1TWdRd2Q2c3cwbHpMWXdKcTdjZERHVU5KV0ZmbGJLUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg==` |
| `JwtSettings__Issuer` | `MobileBackendAPI` |
| `JwtSettings__Audience` | `MobileApp` |

---

## ? **Checklist**

- [ ] Code pushed to GitHub
- [ ] Railway project created
- [ ] GitHub repo connected
- [ ] PostgreSQL database added
- [ ] Environment variables configured
- [ ] JWT keys added
- [ ] Deployment successful
- [ ] Health endpoint working
- [ ] Login endpoint tested

---

## ?? **Expected Result**

After deployment:
- ? API running at `https://your-app.up.railway.app`
- ? Database automatically migrated
- ? 3 users seeded (admin, manager, user)
- ? All endpoints working
- ? JWT authentication working

**Deployment time:** ~5 minutes ??

---

## ?? **Need Help?**

Railway provides:
- Real-time deployment logs
- Database management interface
- Environment variable editor
- Automatic HTTPS
- Custom domains support

**Status:** ? Ready to deploy!

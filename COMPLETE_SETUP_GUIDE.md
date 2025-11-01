# ?? COMPLETE GITHUB & RAILWAY SETUP GUIDE
## For User: mohannadalbujoq-cyber

---

## ? **STEP 1: Add SSH Key to GitHub (2 minutes)**

### **1.1 Copy Your SSH Public Key**
Your SSH key:
```
ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIPPXdnDwJr4v5EEhQeL55BFaiwn8CpXYTdIgYGulzmWd mohannadalbujoq@gmail.com
```

### **1.2 Add to GitHub**
1. **Go to:** https://github.com/settings/keys
2. **Click** "New SSH key"
3. **Title:** `ScanPet Deployment Key`
4. **Key type:** Authentication Key
5. **Paste** the key above
6. **Click** "Add SSH key"

### **1.3 Test SSH Connection**
```powershell
ssh -T git@github.com
```
**Expected output:** `Hi mohannadalbujoq-cyber! You've successfully authenticated...`

---

## ? **STEP 2: Create Private GitHub Repository (1 minute)**

1. **Go to:** https://github.com/new
2. **Fill in:**
   - **Owner:** mohannadalbujoq-cyber
   - **Repository name:** `ScanPet`
   - **Description:** `Pet store mobile backend API - .NET 8 Clean Architecture`
   - **Visibility:** ? **Private**
   - **DO NOT** initialize with README, .gitignore, or license
3. **Click** "Create repository"

**Your repo URL will be:**
```
git@github.com:mohannadalbujoq-cyber/ScanPet.git
```

---

## ? **STEP 3: Run Automated Setup Script (1 minute)**

```powershell
# Run the setup script
cd C:\Users\malbujoq\source\repos\ScanPet
.\setup-github.ps1
```

**The script will:**
1. ? Initialize git
2. ? Configure your user info
3. ? Add all files
4. ? Commit with proper message
5. ? Add remote repository
6. ? Push to GitHub

---

## ? **STEP 4: Deploy to Railway (5 minutes)**

### **4.1 Sign Up for Railway**
1. **Go to:** https://railway.app
2. **Click** "Login with GitHub"
3. **Authorize** Railway to access your GitHub
4. **Grant access** to mohannadalbujoq-cyber account

### **4.2 Create New Project**
1. **Click** "+ New"
2. **Select** "Deploy from GitHub repo"
3. **Choose** your **ScanPet** (private) repository
4. **Railway will analyze** and start building

### **4.3 Configure Environment Variables**

**Click on your service** ? **Variables** tab ? Add these **4 variables:**

```env
ASPNETCORE_ENVIRONMENT=Production
```

```env
ConnectionStrings__DefaultConnection=Host=ep-soft-recipe-a4u2bjus-pooler.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_9zACPHxX4VuZ;SSL Mode=Require
```

```env
JWT_PRIVATE_KEY=PFJTQUtleVZhbHVlPjxNb2R1bHVzPjNJU3FOT2xBZ1hUYld6TGtpT0NhQXRGK2NOVGFIMFhtQmRxWUhmeFBOaDlWMDhZeGRSWEROMWF5L0lkUUhNZldLOFphbXRVaVJjRmVKNlZaZW1RbEZ2UEE4cWllWEZlNi9MS1o0VW9SUVVDaXVPeitZMHJFWFNjN2d0MUM3L0JEMDczNDBDR0d4RGprSlhlUyt1NkJMRGhMTGpMMU9UZlNIU1R3cVdvQkl0VVFpcXIzd3Q1cVZyUEE4dEQ0bDV5d2JneXdJYytSa0l3UUJiVDdQUVRjV24wblRZZnBLb2YvSSt5Qk5yZWNodk9EV0Y5eWxKUEJvQk15Z1pjUFhTMElINHI1WlMwSWtubFRwb0dvQndjbFRaSlZtR0JSQXdOMEo2TlgzTFRoWGU0K1RPN1JsdmhLcVl1TWdRd2Q2c3cwbHpMWXdKcTdjZERHVU5KV0ZmbGJLUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48UD44T2YrMHludkhSc0pRR2JsVjdQZ0QxZ0paN1U2ZHVqbEJwV3VNaWhuTGY1ZWxxTERFUm51R3praDl2UVd0MkRpY3hkd1h6RDZYeWcwT3lGa1VIWUtoY2o5WXF0L1cxT2VFTmVtdUlhT3pVakFURnZudDY3Ym1JUTJtdFFzUGNzLzY3QmpWSHFxWmt1Y1E3elhqRzVXNkoyeTRHZ2RGcVBuaVVBaDhMcEFKSHM9PC9QPjxRPjZsV29Lb3ZDSE5MN2NrWWNZMGVrcmEwTXRtbXoyNTU3eWk2WUZzMjMyZGtjZUNqQjNvQm5wdjBneFRDZWFYS0FXbW9GcmFWTWMxdnUvRnlBbC9tbG9zQzd1VFg1UDR4Q2VBZFVQejBBVmhmUW05NnU3T3dteFF1OUdGUGFKeVlJemhLc0RuU0V2OFRDZVNscXpGbVI1dk4xNzllK2M1SjBPSnR3UzBYQzU2cz08L1E+PERQPml3U2x4YitPTDlLWlBTUGp0ditqRGRlMDNiYjBQUWhhbWJrb28rTXkzNVRKaXMzMEdWdElUMGRoOVR1WUhFeDVnUWNHbnJnSjAzM0UzbFovcC9ybVNadlp0T1hZZ2FBNE4rbnFXTk8xZG50RUZReDRKRVJ3am92Rll3V0xYa0Y2Nm95SVZZalA2bVk1ajlGYTVid0t4UU96NU5IUDcyKzRQampYSFM2dHN4OD08L0RQPjxEUT5wVVNtSnVPTWtxR21YekRkWXBPcnJDV3BHcG13Rk10UWlRYVZremVoVzg1SXl1SUw0UzMwYkl5SDZTcmgvb0tYemF5S3RxNG5IaEVQbXdKdVcwRGh0Y2h5WFN5WXpsM1FuekNrRlRtam5CU050dkMxdERqVkd4R3RXcFFBL3ExUGRtOHEwOE1Qd09RM09CYWkwTjYzRFFoVi9FTFBlbGtuQ0tsdjZGYWFJS009PC9EUT48SW52ZXJzZVE+V3hmbW4rNGhpN3A5N25PSGlFNHNiM0xXQ2NMYkxNTU41RnBQSmgwSGwwaWFSL1JHU3FWdTJKU20zTnNZZnEwSElvNWp6TThXV3hrcTIzTkhzZzl5WXdaUEIxRmdpQ01BSFFOMmd4aWVxaFl2eE8wbWJlVzk3WE1obzhGS2R2WjU5V2JtN0lBTU9tTlMzaXNtUWhmVG5CQytPbkdpdkNYRkk4Mis0aG5RdVRVPTwvSW52ZXJzZVE+PEQ+ZW53dDFtbmc3SWN0K0VBU2RIemN6c2Z5UlZYVk9oMm5JSVZFalJOQ0pEdlBmbXZOdU0wM3U1c3FtOTNFLzlkMy9LU1dXdjRicFVjOHR5Uyt5SjZyZzA0dFZCdVlxbzRJVEtnZmxPV0JKQTkwMHViQWZnSmZnNlp4QmNWRWt1V1BweU1UK3ZkRVZlV0x3OHZoRmdRNE40bWxLS29oVDREVDhtQThWTDJPbC90YzFjUnB0YnZ0OFplYWcrZ1N2b3NISnJ3ZWFmOFc5azdST2RUMHJUQ25rNFVzZTdxUFNDcS9jUHNsZlZWbnhGdVN4U2FxM3NMNDVUbXM0V2FvL0t5cHZCdnZ3R0lOdEcvdDRsbkx1ZzRFYzlLaEhzRVFSRXltY2VhYnc4aFBVRUczbHFWR1gyZjRMaVNYdzlCaThYZ0EreFBEbVUrWlZ2OHVWTG9nUXlhbXlRPT08L0Q+PC9SU0FLZXlWYWx1ZT4=
```

```env
JWT_PUBLIC_KEY=PFJTQUtleVZhbHVlPjxNb2R1bHVzPjNJU3FOT2xBZ1hUYld6TGtpT0NhQXRGK2NOVGFIMFhtQmRxWUhmeFBOaDlWMDhZeGRSWEROMWF5L0lkUUhNZldLOFphbXRVaVJjRmVKNlZaZW1RbEZ2UEE4cWllWEZlNi9MS1o0VW9SUVVDaXVPeitZMHJFWFNjN2d0MUM3L0JEMDczNDBDR0d4RGprSlhlUyt1NkJMRGhMTGpMMU9UZlNIU1R3cVdvQkl0VVFpcXIzd3Q1cVZyUEE4dEQ0bDV5d2JneXdJYytSa0l3UUJiVDdQUVRjV24wblRZZnBLb2YvSSt5Qk5yZWNodk9EV0Y5eWxKUEJvQk15Z1pjUFhTMElINHI1WlMwSWtubFRwb0dvQndjbFRaSlZtR0JSQXdOMEo2TlgzTFRoWGU0K1RPN1JsdmhLcVl1TWdRd2Q2c3cwbHpMWXdKcTdjZERHVU5KV0ZmbGJLUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg==
```

### **4.4 Deploy**
Railway will automatically:
1. ? Detect .NET 8 project
2. ? Build your API
3. ? Connect to Neon database
4. ? Run migrations (already done)
5. ? Give you a URL

---

## ? **STEP 5: Test Deployment (1 minute)**

**Your Railway URL will be:**
```
https://scanpet-production-xxxx.up.railway.app
```

**Test it:**
```powershell
# Health check
curl https://your-railway-url.up.railway.app/health

# Login
curl -X POST https://your-railway-url.up.railway.app/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin@123"}'
```

---

## ?? **COMPLETE CHECKLIST**

- [ ] **Step 1:** Add SSH key to GitHub
- [ ] **Step 2:** Create private repository
- [ ] **Step 3:** Run setup-github.ps1 script
- [ ] **Step 4:** Deploy to Railway
- [ ] **Step 5:** Test deployment

---

## ?? **Total Time: ~10 minutes**

**What you get:**
- ? Private GitHub repository
- ? Deployed API on Railway
- ? Neon PostgreSQL database
- ? JWT authentication
- ? 3 users ready (admin, manager, user)
- ? All 33+ endpoints working

---

## ?? **Troubleshooting**

### **SSH Key Issues:**
```powershell
# Start SSH agent
ssh-agent
ssh-add ~/.ssh/id_ed25519

# Test connection
ssh -T git@github.com
```

### **Git Push Fails:**
```powershell
# Remove remote and try again
git remote remove origin
git remote add origin git@github.com:mohannadalbujoq-cyber/ScanPet.git
git push -u origin main
```

### **Railway Build Fails:**
Check:
1. All 4 environment variables are set
2. Variables have no extra spaces
3. Connection string is correct

---

## ? **READY TO START!**

**Run this command to begin:**
```powershell
cd C:\Users\malbujoq\source\repos\ScanPet
.\setup-github.ps1
```

Then follow the Railway steps above! ??

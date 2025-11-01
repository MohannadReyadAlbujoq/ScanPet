# ?? CLOUD DEPLOYMENT GUIDE (Azure, AWS, DigitalOcean)

**Date:** December 2024  
**Purpose:** Deploy backend + database to cloud  
**Platforms:** Azure, AWS, DigitalOcean

---

## ?? DEPLOYMENT OPTIONS OVERVIEW

| Platform | Backend | Database | Cost/Month | Difficulty |
|----------|---------|----------|------------|------------|
| **Azure** | App Service | Azure Database for PostgreSQL | ~$20-50 | Easy |
| **AWS** | Elastic Beanstalk | RDS PostgreSQL | ~$25-60 | Medium |
| **DigitalOcean** | App Platform | Managed PostgreSQL | ~$15-40 | Easy |
| **Heroku** | Dyno | Heroku Postgres | ~$7-25 | Very Easy |

---

## ?? OPTION 1: MICROSOFT AZURE

### Prerequisites
- Azure account
- Azure CLI installed
- .NET SDK installed

### Step 1: Create Azure Resources

```bash
# Login to Azure
az login

# Create resource group
az group create --name ScanPetRG --location eastus

# Create PostgreSQL database
az postgres flexible-server create \
    --resource-group ScanPetRG \
    --name scanpet-db \
    --location eastus \
    --admin-user scanpetadmin \
    --admin-password "YourSecurePassword123!" \
    --sku-name Standard_B1ms \
    --tier Burstable \
    --storage-size 32 \
    --version 14

# Create database
az postgres flexible-server db create \
    --resource-group ScanPetRG \
    --server-name scanpet-db \
    --database-name ScanPetDB

# Configure firewall to allow Azure services
az postgres flexible-server firewall-rule create \
    --resource-group ScanPetRG \
    --name scanpet-db \
    --rule-name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

# Create App Service Plan
az appservice plan create \
    --name ScanPetPlan \
    --resource-group ScanPetRG \
    --sku B1 \
    --is-linux

# Create Web App
az webapp create \
    --resource-group ScanPetRG \
    --plan ScanPetPlan \
    --name scanpet-backend \
    --runtime "DOTNETCORE:8.0"
```

### Step 2: Configure Connection String

```bash
# Set connection string as environment variable
az webapp config connection-string set \
    --resource-group ScanPetRG \
    --name scanpet-backend \
    --settings DefaultConnection="Host=scanpet-db.postgres.database.azure.com;Port=5432;Database=ScanPetDB;Username=scanpetadmin;Password=YourSecurePassword123!;SSL Mode=Require" \
    --connection-string-type PostgreSQL
```

### Step 3: Configure App Settings

```bash
# Set environment variables
az webapp config appsettings set \
    --resource-group ScanPetRG \
    --name scanpet-backend \
    --settings \
        ASPNETCORE_ENVIRONMENT=Production \
        Jwt__Issuer=ScanPetMobileBackend \
        Jwt__Audience=ScanPetMobileApp \
        Jwt__AccessTokenExpiryMinutes=15
```

### Step 4: Deploy Application

```bash
# Build and publish
cd src/API/MobileBackend.API
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .

# Deploy to Azure
az webapp deployment source config-zip \
    --resource-group ScanPetRG \
    --name scanpet-backend \
    --src ../deploy.zip
```

### Step 5: Configure Custom Domain (Optional)

```bash
# Add custom domain
az webapp config hostname add \
    --resource-group ScanPetRG \
    --webapp-name scanpet-backend \
    --hostname api.scanpet.com

# Enable HTTPS
az webapp config ssl bind \
    --resource-group ScanPetRG \
    --name scanpet-backend \
    --certificate-thumbprint <thumbprint> \
    --ssl-type SNI
```

### Step 6: Verify Deployment

```bash
# Check logs
az webapp log tail \
    --resource-group ScanPetRG \
    --name scanpet-backend

# Test API
curl https://scanpet-backend.azurewebsites.net/health
curl https://scanpet-backend.azurewebsites.net/swagger
```

### Azure Costs (Monthly Estimate)

- **App Service (B1):** $13
- **PostgreSQL (Burstable B1ms):** $15
- **Storage:** $5
- **Bandwidth:** $5
- **Total:** ~$38/month

---

## ?? OPTION 2: AMAZON AWS

### Prerequisites
- AWS account
- AWS CLI installed
- EB CLI installed

### Step 1: Create RDS PostgreSQL Database

```bash
# Create RDS PostgreSQL instance
aws rds create-db-instance \
    --db-instance-identifier scanpet-db \
    --db-instance-class db.t3.micro \
    --engine postgres \
    --engine-version 14.7 \
    --master-username scanpetadmin \
    --master-user-password YourSecurePassword123! \
    --allocated-storage 20 \
    --storage-type gp2 \
    --vpc-security-group-ids sg-xxxxxxxxx \
    --db-name ScanPetDB \
    --backup-retention-period 7 \
    --publicly-accessible

# Wait for instance to be available
aws rds wait db-instance-available \
    --db-instance-identifier scanpet-db

# Get connection endpoint
aws rds describe-db-instances \
    --db-instance-identifier scanpet-db \
    --query 'DBInstances[0].Endpoint.Address' \
    --output text
```

### Step 2: Initialize Elastic Beanstalk

```bash
# Navigate to project
cd src/API/MobileBackend.API

# Initialize EB
eb init -p "64bit Amazon Linux 2 v2.5.3 running .NET 8" \
    -r us-east-1 \
    scanpet-backend

# Create environment
eb create scanpet-production \
    --instance-type t3.small \
    --database \
    --database.engine postgres \
    --database.username scanpetadmin \
    --database.password YourSecurePassword123!
```

### Step 3: Configure Environment Variables

```bash
# Set environment variables
eb setenv \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Host=scanpet-db.xxxxxx.us-east-1.rds.amazonaws.com;Port=5432;Database=ScanPetDB;Username=scanpetadmin;Password=YourSecurePassword123!;SSL Mode=Require" \
    Jwt__Issuer=ScanPetMobileBackend \
    Jwt__Audience=ScanPetMobileApp
```

### Step 4: Deploy Application

```bash
# Create deployment package
dotnet publish -c Release

# Deploy to EB
eb deploy
```

### Step 5: Configure HTTPS

```bash
# Request SSL certificate
aws acm request-certificate \
    --domain-name api.scanpet.com \
    --validation-method DNS \
    --region us-east-1

# Update load balancer to use HTTPS
eb config
# Add HTTPS listener in configuration file
```

### Step 6: Verify Deployment

```bash
# Open application
eb open

# Check logs
eb logs

# Test API
curl https://scanpet-production.us-east-1.elasticbeanstalk.com/health
```

### AWS Costs (Monthly Estimate)

- **EC2 (t3.small):** $15
- **RDS (db.t3.micro):** $15
- **Load Balancer:** $18
- **Storage:** $5
- **Data Transfer:** $5
- **Total:** ~$58/month

---

## ?? OPTION 3: DIGITALOCEAN (Recommended for Beginners)

### Prerequisites
- DigitalOcean account
- doctl CLI installed

### Step 1: Create Managed PostgreSQL Database

```bash
# Login
doctl auth init

# Create PostgreSQL cluster
doctl databases create scanpet-db \
    --engine pg \
    --version 14 \
    --region nyc3 \
    --size db-s-1vcpu-1gb \
    --num-nodes 1

# Get connection details
doctl databases connection scanpet-db

# Create database
doctl databases db create scanpet-db ScanPetDB
```

### Step 2: Create App Platform Application

**Using DigitalOcean Console (Easier):**

1. Go to https://cloud.digitalocean.com/apps
2. Click "Create App"
3. Select "GitHub" and authorize
4. Choose your repository
5. Configure:
   - **Resource Type:** Web Service
   - **Branch:** main
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Run Command:** `dotnet publish/MobileBackend.API.dll`
   - **HTTP Port:** 5000

**Or Using CLI:**

```bash
# Create app spec
cat > app.yaml << EOF
name: scanpet-backend
region: nyc
services:
- name: api
  github:
    repo: your-username/ScanPet
    branch: main
    deploy_on_push: true
  build_command: dotnet publish -c Release -o ./publish
  run_command: dotnet publish/MobileBackend.API.dll
  environment_slug: dotnet-core
  instance_count: 1
  instance_size_slug: basic-xxs
  http_port: 5000
  envs:
  - key: ASPNETCORE_ENVIRONMENT
    value: Production
  - key: ConnectionStrings__DefaultConnection
    value: ${scanpet-db.DATABASE_URL}
  - key: Jwt__Issuer
    value: ScanPetMobileBackend
databases:
- name: scanpet-db
  engine: PG
  version: "14"
EOF

# Create app
doctl apps create --spec app.yaml
```

### Step 3: Configure Environment Variables

```bash
# List apps
doctl apps list

# Get app ID
APP_ID=$(doctl apps list --format ID --no-header)

# Update environment variables
doctl apps update $APP_ID \
    --spec - << EOF
spec:
  envs:
  - key: ConnectionStrings__DefaultConnection
    value: "Host=your-db-host;Port=25060;Database=ScanPetDB;Username=doadmin;Password=xxxxx;SSL Mode=Require"
  - key: Jwt__AccessTokenExpiryMinutes
    value: "15"
EOF
```

### Step 4: Deploy

```bash
# Trigger deployment
doctl apps create-deployment $APP_ID

# Monitor deployment
doctl apps logs $APP_ID --follow
```

### Step 5: Configure Custom Domain

```bash
# Add domain
doctl apps update $APP_ID --spec - << EOF
spec:
  domains:
  - domain: api.scanpet.com
    type: PRIMARY
EOF

# Update DNS records (in your domain provider)
# Add CNAME: api.scanpet.com -> your-app.ondigitalocean.app
```

### DigitalOcean Costs (Monthly Estimate)

- **App Platform (Basic):** $5
- **Managed PostgreSQL (1GB):** $15
- **Bandwidth:** Included
- **Total:** ~$20/month ? **Best Value**

---

## ?? OPTION 4: HEROKU (Easiest)

### Prerequisites
- Heroku account
- Heroku CLI installed

### Complete Deployment (5 minutes)

```bash
# Login
heroku login

# Create app
heroku create scanpet-backend

# Add PostgreSQL
heroku addons:create heroku-postgresql:mini

# Get database URL
heroku config:get DATABASE_URL

# Set buildpack
heroku buildpacks:set heroku/dotnet

# Configure environment
heroku config:set \
    ASPNETCORE_ENVIRONMENT=Production \
    Jwt__Issuer=ScanPetMobileBackend \
    Jwt__Audience=ScanPetMobileApp

# Deploy
git push heroku main

# Run migrations
heroku run dotnet ef database update --project src/Infrastructure/MobileBackend.Infrastructure --startup-project src/API/MobileBackend.API

# Open app
heroku open
```

### Heroku Costs (Monthly)

- **Dyno (Eco):** $5
- **PostgreSQL (Mini):** $5
- **Total:** ~$10/month ? **Cheapest**

---

## ??? DATABASE MIGRATION

### Run Migrations After Deployment

**Azure:**
```bash
# Connect to database
dotnet ef database update \
    --connection "Host=scanpet-db.postgres.database.azure.com;..."
```

**AWS:**
```bash
# SSH into EC2
eb ssh scanpet-production

# Run migrations
cd /var/app/current
dotnet ef database update
```

**DigitalOcean:**
```bash
# Use console to run command
doctl apps exec $APP_ID --component api -- \
    dotnet ef database update
```

### Seed Initial Data

```bash
# The DbSeeder runs automatically on first startup
# Or manually trigger:
curl -X POST https://your-api.com/api/admin/seed
```

---

## ?? POST-DEPLOYMENT SECURITY

### 1. Configure HTTPS Only

```bash
# Force HTTPS redirect (already in code)
# Verify in appsettings.Production.json
"UseHttpsRedirection": true
```

### 2. Update CORS

```json
// appsettings.Production.json
"Cors": {
  "AllowedOrigins": [
    "https://scanpet.com",
    "https://app.scanpet.com"
  ]
}
```

### 3. Disable Swagger

```json
"Features": {
  "EnableSwagger": false  // In production
}
```

### 4. Configure Firewall

**Azure:**
```bash
# Restrict database access to App Service only
az postgres flexible-server firewall-rule create \
    --name AllowAppService \
    --start-ip-address <app-service-ip> \
    --end-ip-address <app-service-ip>
```

**AWS:**
```bash
# Update security group
aws ec2 authorize-security-group-ingress \
    --group-id sg-xxxxx \
    --protocol tcp \
    --port 5432 \
    --source-group sg-xxxxx  # App security group
```

---

## ?? MONITORING SETUP

### Application Insights (Azure)

```bash
# Enable Application Insights
az monitor app-insights component create \
    --app scanpet-insights \
    --location eastus \
    --resource-group ScanPetRG

# Get instrumentation key
az monitor app-insights component show \
    --app scanpet-insights \
    --resource-group ScanPetRG \
    --query instrumentationKey

# Set in App Service
az webapp config appsettings set \
    --settings APPINSIGHTS_INSTRUMENTATIONKEY=<key>
```

### CloudWatch (AWS)

```bash
# CloudWatch is automatically enabled
# View logs
aws logs tail /aws/elasticbeanstalk/scanpet-production --follow
```

---

## ?? VERIFICATION CHECKLIST

### After Deployment:
- [ ] API health check works: `/health`
- [ ] Swagger accessible (dev only): `/swagger`
- [ ] Database connection successful
- [ ] Migrations applied
- [ ] Seed data loaded
- [ ] JWT tokens working
- [ ] HTTPS enforced
- [ ] CORS configured correctly
- [ ] Environment variables set
- [ ] Logs accessible
- [ ] Monitoring enabled

### Test All Endpoints:
```bash
# Health check
curl https://your-api.com/health

# Register user
curl -X POST https://your-api.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test@123"}'

# Login
curl -X POST https://your-api.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@scanpet.com","password":"Admin@123"}'

# Get colors (with token)
curl -X GET https://your-api.com/api/colors \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ?? COST COMPARISON SUMMARY

| Platform | Monthly | Best For |
|----------|---------|----------|
| **Heroku** | $10 | Testing, MVPs |
| **DigitalOcean** | $20 | Small apps, startups |
| **Azure** | $38 | Enterprise, Microsoft stack |
| **AWS** | $58 | Scalability, full AWS ecosystem |

**Recommendation:** Start with **DigitalOcean** for best value + ease.

---

## ?? DEPLOYMENT SUMMARY

**Easiest:** Heroku (5 minutes)  
**Best Value:** DigitalOcean ($20/month)  
**Most Features:** Azure (Microsoft ecosystem)  
**Most Scalable:** AWS (enterprise-grade)

---

**Status:** ? **COMPLETE CLOUD DEPLOYMENT GUIDE**  
**Platforms:** Azure, AWS, DigitalOcean, Heroku  
**Deployment Time:** 15-30 minutes

---

**END OF CLOUD DEPLOYMENT GUIDE**

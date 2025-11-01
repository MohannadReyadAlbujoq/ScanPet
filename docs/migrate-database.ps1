# Database Migration and Seeding Script
# Run this script to apply migrations and seed the database

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ScanPet Database Migration & Seeding" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to API project
$apiProject = "src\API\MobileBackend.API"
$infraProject = "src\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj"

Write-Host "? Checking connection to Neon PostgreSQL..." -ForegroundColor Yellow

# Check if EF Core tools are installed
try {
    $efVersion = dotnet ef --version
    Write-Host "? EF Core Tools: $efVersion" -ForegroundColor Green
} catch {
    Write-Host "? EF Core Tools not found. Installing..." -ForegroundColor Red
    dotnet tool install --global dotnet-ef
}

Write-Host ""
Write-Host "? Creating migration..." -ForegroundColor Yellow
Set-Location $apiProject

# Create initial migration
dotnet ef migrations add InitialCreate --project $infraProject --startup-project . --context ApplicationDbContext

Write-Host ""
Write-Host "? Applying migrations to database..." -ForegroundColor Yellow

# Apply migrations
dotnet ef database update --project $infraProject --startup-project . --context ApplicationDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ? SUCCESS!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Database migrated and seeded successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "? Admin Account:" -ForegroundColor Cyan
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Password: Admin@123456" -ForegroundColor White
    Write-Host "  Email: admin@scanpet.com" -ForegroundColor White
    Write-Host ""
    Write-Host "? Manager Account:" -ForegroundColor Cyan
    Write-Host "  Username: manager" -ForegroundColor White
    Write-Host "  Password: Manager@123456" -ForegroundColor White
    Write-Host "  Email: manager@scanpet.com" -ForegroundColor White
    Write-Host ""
    Write-Host "? Seeded Data:" -ForegroundColor Cyan
    Write-Host "  - 26 Permissions" -ForegroundColor White
    Write-Host "  - 3 Roles (Administrator, Manager, User)" -ForegroundColor White
    Write-Host "  - 2 Users (admin, manager)" -ForegroundColor White
    Write-Host "  - 8 Colors" -ForegroundColor White
    Write-Host "  - 4 Locations" -ForegroundColor White
    Write-Host "  - 3 Sample Items" -ForegroundColor White
    Write-Host "  - 1 Sample Order" -ForegroundColor White
    Write-Host ""
    Write-Host "? Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Run the API: dotnet run" -ForegroundColor White
    Write-Host "  2. Open Swagger: http://localhost:5000/swagger" -ForegroundColor White
    Write-Host "  3. Login with admin credentials" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "? Migration failed! Check errors above." -ForegroundColor Red
    Write-Host ""
}

# Return to original directory
Set-Location ..\..\..\

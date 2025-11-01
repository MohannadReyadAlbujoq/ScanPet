# Run API and Complete Database Seeding
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Running API & Completing Seeding" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

cd "C:\Users\malbujoq\source\repos\ScanPet\src\API\MobileBackend.API"

Write-Host "Starting API..." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop after seeding completes" -ForegroundColor Yellow
Write-Host ""

dotnet run --urls="http://localhost:5000"

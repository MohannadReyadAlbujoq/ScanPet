# Generate JWT RSA Keys for Production (Compatible with all .NET versions)
# Run this script to generate RSA-2048 keys

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  JWT RSA Key Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    # Generate RSA key pair (2048-bit for security)
    $rsa = New-Object System.Security.Cryptography.RSACryptoServiceProvider(2048)
    
    # Export keys as XML (compatible with all .NET versions)
    $privateKeyXml = $rsa.ToXmlString($true)  # true = include private parameters
    $publicKeyXml = $rsa.ToXmlString($false)  # false = public key only
    
    # Convert to Base64 for storage
    $privateKeyBase64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($privateKeyXml))
    $publicKeyBase64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($publicKeyXml))
    
    Write-Host "? RSA Keys Generated Successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "  PRIVATE KEY (Base64 - Keep Secret!)" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host $privateKeyBase64 -ForegroundColor White
    Write-Host ""
    
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  PUBLIC KEY (Base64)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host $publicKeyBase64 -ForegroundColor White
    Write-Host ""
    
    # Save to file
    $keysFile = "jwt-keys.txt"
    $content = @"
========================================
JWT RSA KEYS - GENERATED $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
========================================

??  SECURITY WARNING: Keep these keys secure!
   - Never commit to source control
   - Store in environment variables or secrets manager in production
   - Rotate keys every 90 days

========================================
PRIVATE KEY (Base64) - For appsettings.json
========================================
$privateKeyBase64

========================================
PUBLIC KEY (Base64) - For appsettings.json
========================================
$publicKeyBase64

========================================
USAGE IN appsettings.json
========================================
"JwtSettings": {
  "PrivateKey": "$privateKeyBase64",
  "PublicKey": "$publicKeyBase64",
  "Issuer": "MobileBackendAPI",
  "Audience": "MobileApp",
  "AccessTokenExpiryMinutes": 15,
  "RefreshTokenExpiryDays": 7
}

========================================
ENVIRONMENT VARIABLES (Production)
========================================
JwtSettings__PrivateKey=$privateKeyBase64
JwtSettings__PublicKey=$publicKeyBase64

========================================
"@
    
    $content | Out-File -FilePath $keysFile -Encoding UTF8
    
    Write-Host "? Keys saved to: $keysFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "? Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Keys are already configured in appsettings.json!" -ForegroundColor White
    Write-Host "2. Run: cd src\API\MobileBackend.API; dotnet run" -ForegroundColor White
    Write-Host "3. Database will auto-seed on startup!" -ForegroundColor White
    Write-Host ""
    
    # Automatically update appsettings.json
    Write-Host "? Updating appsettings.json automatically..." -ForegroundColor Cyan
    
    $appSettingsPath = "src\API\MobileBackend.API\appsettings.json"
    if (Test-Path $appSettingsPath) {
        $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        $appSettings.JwtSettings.PrivateKey = $privateKeyBase64
        $appSettings.JwtSettings.PublicKey = $publicKeyBase64
        $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
        Write-Host "? appsettings.json updated successfully!" -ForegroundColor Green
    }
    
    $appSettingsDevPath = "src\API\MobileBackend.API\appsettings.Development.json"
    if (Test-Path $appSettingsDevPath) {
        $appSettingsDev = Get-Content $appSettingsDevPath -Raw | ConvertFrom-Json
        $appSettingsDev.JwtSettings.PrivateKey = $privateKeyBase64
        $appSettingsDev.JwtSettings.PublicKey = $publicKeyBase64
        $appSettingsDev | ConvertTo-Json -Depth 10 | Set-Content $appSettingsDevPath
        Write-Host "? appsettings.Development.json updated successfully!" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "? ALL DONE! You can now run the API:" -ForegroundColor Green
    Write-Host "   cd src\API\MobileBackend.API" -ForegroundColor White
    Write-Host "   dotnet run" -ForegroundColor White
    Write-Host ""
}
catch {
    Write-Host "? Error generating keys: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

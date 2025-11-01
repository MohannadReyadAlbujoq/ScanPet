# ScanPet GitHub Setup Script
# Run this AFTER creating the repository on GitHub

$ErrorActionPreference = "Stop"

Write-Host "=== ScanPet GitHub Setup ===" -ForegroundColor Cyan
Write-Host ""

# Configuration
$GITHUB_USERNAME = "mohannadalbujoq-cyber"
$REPO_NAME = "ScanPet"
$REPO_URL = "git@github.com:$GITHUB_USERNAME/$REPO_NAME.git"

Write-Host "GitHub Username: $GITHUB_USERNAME" -ForegroundColor Yellow
Write-Host "Repository Name: $REPO_NAME" -ForegroundColor Yellow
Write-Host "Repository URL: $REPO_URL" -ForegroundColor Yellow
Write-Host ""

# Navigate to project
Write-Host "Navigating to project directory..." -ForegroundColor Green
cd C:\Users\malbujoq\source\repos\ScanPet

# Initialize git (if not already)
if (-not (Test-Path ".git")) {
    Write-Host "Initializing git repository..." -ForegroundColor Green
    git init
} else {
    Write-Host "Git repository already initialized" -ForegroundColor Yellow
}

# Configure git user
Write-Host "Configuring git user..." -ForegroundColor Green
git config user.name "mohannadalbujoq-cyber"
git config user.email "mohannadalbujoq@gmail.com"

# Add all files
Write-Host "Adding all files..." -ForegroundColor Green
git add .

# Commit
Write-Host "Committing changes..." -ForegroundColor Green
git commit -m "Initial commit: Production-ready ScanPet API with Clean Architecture, JWT authentication, and Neon PostgreSQL"

# Check if remote exists
$remoteExists = git remote | Select-String -Pattern "origin"
if ($remoteExists) {
    Write-Host "Removing existing remote..." -ForegroundColor Yellow
    git remote remove origin
}

# Add remote
Write-Host "Adding remote repository..." -ForegroundColor Green
git remote add origin $REPO_URL

# Set main branch
Write-Host "Setting main branch..." -ForegroundColor Green
git branch -M main

# Push to GitHub
Write-Host ""
Write-Host "Pushing to GitHub..." -ForegroundColor Green
Write-Host "This may prompt for SSH key passphrase (if you set one)" -ForegroundColor Yellow
Write-Host ""

git push -u origin main

Write-Host ""
Write-Host "=== Setup Complete! ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your repository is now available at:" -ForegroundColor Green
Write-Host "https://github.com/$GITHUB_USERNAME/$REPO_NAME" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Go to https://railway.app" -ForegroundColor White
Write-Host "2. Sign in with GitHub" -ForegroundColor White
Write-Host "3. Deploy from your $REPO_NAME repository" -ForegroundColor White
Write-Host ""

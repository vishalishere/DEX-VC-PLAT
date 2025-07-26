# DecVCPlat Personal Machine Quick Setup Script
# Final version that uses the most accurate solution file

Write-Host "DecVCPlat Personal Machine Setup Starting..." -ForegroundColor Green

# Check prerequisites
Write-Host "Checking Prerequisites..." -ForegroundColor Yellow

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK Found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host ".NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# Check Node.js
try {
    $nodeVersion = node --version
    Write-Host "Node.js Found: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "Node.js not found. Please install Node.js 18+" -ForegroundColor Red
    exit 1
}

# Check Docker
try {
    $dockerVersion = docker --version
    Write-Host "Docker Found: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "Docker not found. Docker optional for local development" -ForegroundColor Yellow
}

Write-Host "Setting up DecVCPlat Solution..." -ForegroundColor Yellow

# Use the accurate solution file
Write-Host "Using accurate solution file (DecVCPlat.Accurate.sln)..." -ForegroundColor Cyan

# Restore .NET packages
Write-Host "Restoring .NET packages..." -ForegroundColor Cyan
dotnet restore DecVCPlat.Accurate.sln

# Setup frontend
Write-Host "Setting up React frontend..." -ForegroundColor Cyan
if (Test-Path -Path "Frontend\DecVCPlat.Web") {
    Set-Location Frontend\DecVCPlat.Web
    npm install
    Set-Location ..\..
} elseif (Test-Path -Path "Frontend\src") {
    Set-Location Frontend\src
    npm install
    Set-Location ..\..
} else {
    Write-Host "Frontend directory structure not as expected. Skipping npm install." -ForegroundColor Yellow
}

# Create local database (if SQL Server available)
Write-Host "Setting up local database..." -ForegroundColor Cyan
Write-Host "Note: Update connection strings in appsettings.json files for your local SQL Server" -ForegroundColor Yellow

# Build solution
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build DecVCPlat.Accurate.sln

Write-Host "DecVCPlat Setup Complete!" -ForegroundColor Green
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Update connection strings in appsettings.json files" -ForegroundColor White
Write-Host "  2. Run database migrations: dotnet ef database update" -ForegroundColor White
Write-Host "  3. Start services: dotnet run (for each service)" -ForegroundColor White
Write-Host "  4. Start frontend: cd frontend && npm start" -ForegroundColor White
Write-Host "  5. Visit http://localhost:3000" -ForegroundColor White
Write-Host "Ready for development!" -ForegroundColor Green

# Setup Development Environment Script for DecVCPlat
# This script sets up the local development environment for the DecVCPlat project

# Ensure the script stops on first error
$ErrorActionPreference = "Stop"

Write-Host "Setting up DecVCPlat development environment..." -ForegroundColor Green

# Create .env file from template if it doesn't exist
if (-not (Test-Path "../.env")) {
    Write-Host "Creating .env file from template..." -ForegroundColor Yellow
    Copy-Item "../.env.template" "../.env"
    Write-Host ".env file created. Please update it with your specific configuration." -ForegroundColor Yellow
}

# Check if Docker is installed
try {
    docker --version
    $dockerInstalled = $true
    Write-Host "Docker is installed." -ForegroundColor Green
}
catch {
    $dockerInstalled = $false
    Write-Host "Docker is not installed or not in PATH. Some features will be limited." -ForegroundColor Yellow
}

# Check if .NET SDK is installed
try {
    dotnet --version
    $dotnetInstalled = $true
    Write-Host ".NET SDK is installed." -ForegroundColor Green
}
catch {
    $dotnetInstalled = $false
    Write-Host ".NET SDK is not installed or not in PATH. Please install .NET SDK to continue." -ForegroundColor Red
    exit 1
}

# Check if Node.js is installed
try {
    node --version
    $nodeInstalled = $true
    Write-Host "Node.js is installed." -ForegroundColor Green
}
catch {
    $nodeInstalled = $false
    Write-Host "Node.js is not installed or not in PATH. Frontend development will be limited." -ForegroundColor Yellow
}

# Create necessary directories
Write-Host "Creating necessary directories..." -ForegroundColor Green
$directories = @(
    "../logs",
    "../data"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
        Write-Host "Created directory: $dir" -ForegroundColor Green
    }
}

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Green
Set-Location ..
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore NuGet packages." -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host "Building solution..." -ForegroundColor Green
dotnet build --configuration Debug --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build solution." -ForegroundColor Red
    exit 1
}

# Setup LocalDB if Docker is not available
if (-not $dockerInstalled) {
    Write-Host "Setting up SQL Server LocalDB..." -ForegroundColor Green
    
    # Check if LocalDB is installed
    try {
        sqllocaldb info
        $localDbInstalled = $true
        Write-Host "SQL Server LocalDB is installed." -ForegroundColor Green
    }
    catch {
        $localDbInstalled = $false
        Write-Host "SQL Server LocalDB is not installed. Please install SQL Server LocalDB to continue." -ForegroundColor Red
        exit 1
    }
    
    # Create LocalDB instance if it doesn't exist
    $localDbInstance = "DecVCPlat"
    $localDbInfo = sqllocaldb info $localDbInstance 2>$null
    
    if (-not $localDbInfo) {
        Write-Host "Creating LocalDB instance '$localDbInstance'..." -ForegroundColor Yellow
        sqllocaldb create $localDbInstance
        sqllocaldb start $localDbInstance
        Write-Host "LocalDB instance '$localDbInstance' created and started." -ForegroundColor Green
    }
    else {
        Write-Host "LocalDB instance '$localDbInstance' already exists." -ForegroundColor Green
        sqllocaldb start $localDbInstance 2>$null
    }
    
    # Update appsettings.json files with LocalDB connection strings
    $services = @(
        "src/Backend/DecVCPlat.UserManagement",
        "src/Backend/DecVCPlat.ProjectManagement",
        "src/Backend/DecVCPlat.Voting",
        "src/Backend/DecVCPlat.Funding",
        "src/Backend/DecVCPlat.Notification"
    )
    
    foreach ($service in $services) {
        $appSettingsPath = "$service/appsettings.json"
        if (Test-Path $appSettingsPath) {
            Write-Host "Updating connection string in $appSettingsPath..." -ForegroundColor Yellow
            
            $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
            $dbName = "DecVCPlat_" + ($service -split "\.")[-1]
            
            if (-not $appSettings.ConnectionStrings) {
                $appSettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
            }
            
            $appSettings.ConnectionStrings.DefaultConnection = "Server=(localdb)\$localDbInstance;Database=$dbName;Trusted_Connection=True;MultipleActiveResultSets=true"
            
            $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
            Write-Host "Updated connection string in $appSettingsPath." -ForegroundColor Green
        }
    }
}

# Setup frontend if Node.js is installed
if ($nodeInstalled -and (Test-Path "src/Frontend")) {
    Write-Host "Setting up frontend..." -ForegroundColor Green
    Set-Location src/Frontend
    
    # Install npm packages
    Write-Host "Installing npm packages..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install npm packages." -ForegroundColor Red
        Set-Location ../..
    }
    else {
        Write-Host "npm packages installed successfully." -ForegroundColor Green
        Set-Location ../..
    }
}

# Create development certificates for HTTPS
Write-Host "Creating development certificates for HTTPS..." -ForegroundColor Green
dotnet dev-certs https --clean
dotnet dev-certs https --trust
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create development certificates." -ForegroundColor Red
}
else {
    Write-Host "Development certificates created successfully." -ForegroundColor Green
}

# Instructions for running the application
Write-Host "`nSetup completed successfully!" -ForegroundColor Green
Write-Host "`nTo run the application:" -ForegroundColor Cyan
if ($dockerInstalled) {
    Write-Host "1. Using Docker Compose (recommended):" -ForegroundColor Cyan
    Write-Host "   docker-compose up" -ForegroundColor White
}
Write-Host "2. Using .NET CLI (individual services):" -ForegroundColor Cyan
Write-Host "   - Open multiple terminal windows" -ForegroundColor White
Write-Host "   - For each service, run: dotnet run --project src/Backend/DecVCPlat.ServiceName" -ForegroundColor White
Write-Host "   - For the frontend, run: cd src/Frontend && npm start" -ForegroundColor White

Write-Host "`nAccess the services at:" -ForegroundColor Cyan
Write-Host "- API Gateway: https://localhost:5001" -ForegroundColor White
Write-Host "- User Management: http://localhost:5010" -ForegroundColor White
Write-Host "- Project Management: http://localhost:5020" -ForegroundColor White
Write-Host "- Voting: http://localhost:5030" -ForegroundColor White
Write-Host "- Funding: http://localhost:5040" -ForegroundColor White
Write-Host "- Notification: http://localhost:5050" -ForegroundColor White
Write-Host "- Frontend: http://localhost:3000" -ForegroundColor White

Write-Host "`nHappy coding!" -ForegroundColor Green

# DecVCPlat Complete Environment Setup Script
# This script sets up the complete development environment for DecVCPlat

param (
    [Parameter(Mandatory=$false)]
    [switch]$UseDocker = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$SetupLocalDb = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$BuildImages = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$StartServices = $false
)

# Ensure the script stops on first error
$ErrorActionPreference = "Stop"

# Set working directory to repository root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $scriptPath "..")
$rootDir = Get-Location

function Write-Step {
    param (
        [string]$Message
    )
    Write-Host "`n==== $Message ====`n" -ForegroundColor Cyan
}

function Write-Success {
    param (
        [string]$Message
    )
    Write-Host $Message -ForegroundColor Green
}

function Write-Warning {
    param (
        [string]$Message
    )
    Write-Host $Message -ForegroundColor Yellow
}

function Write-Error {
    param (
        [string]$Message
    )
    Write-Host $Message -ForegroundColor Red
}

function Check-Command {
    param (
        [string]$Command,
        [string]$Name
    )
    
    try {
        Invoke-Expression "$Command" | Out-Null
        Write-Success "$Name is installed."
        return $true
    }
    catch {
        Write-Warning "$Name is not installed or not in PATH."
        return $false
    }
}

# Check prerequisites
Write-Step "Checking Prerequisites"

$dotnetInstalled = Check-Command "dotnet --version" ".NET SDK"
if (-not $dotnetInstalled) {
    Write-Error "Please install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
}

$nodeInstalled = Check-Command "node --version" "Node.js"
if (-not $nodeInstalled) {
    Write-Warning "Node.js is not installed. Frontend development will be limited."
    Write-Warning "Install Node.js from https://nodejs.org/"
}

$dockerInstalled = Check-Command "docker --version" "Docker"
if ($UseDocker -and -not $dockerInstalled) {
    Write-Error "Docker is required for Docker-based setup. Please install Docker Desktop."
    Write-Error "https://www.docker.com/products/docker-desktop"
    exit 1
}

if ($UseDocker) {
    $dockerComposeInstalled = Check-Command "docker-compose --version" "Docker Compose"
    if (-not $dockerComposeInstalled) {
        Write-Error "Docker Compose is required. It should be included with Docker Desktop."
        exit 1
    }
}

if ($SetupLocalDb -or -not $UseDocker) {
    $localDbInstalled = Check-Command "sqllocaldb version" "SQL Server LocalDB"
    if (-not $localDbInstalled) {
        Write-Warning "SQL Server LocalDB is not installed. It's required for local database development."
        Write-Warning "Install SQL Server Express with LocalDB: https://www.microsoft.com/en-us/sql-server/sql-server-downloads"
    }
}

# Create necessary directories
Write-Step "Creating Necessary Directories"
$directories = @(
    "logs",
    "data"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
        Write-Success "Created directory: $dir"
    }
    else {
        Write-Success "Directory already exists: $dir"
    }
}

# Create .env file from template if it doesn't exist
Write-Step "Setting Up Environment Configuration"
if (-not (Test-Path ".env")) {
    if (Test-Path ".env.template") {
        Copy-Item ".env.template" ".env"
        Write-Success ".env file created from template. Please update it with your specific configuration."
    }
    else {
        # Create a basic .env file
        @"
# DecVCPlat Environment Configuration
ASPNETCORE_ENVIRONMENT=Development
SQL_SERVER_PASSWORD=DecVCPlat_Strong!Password
JWT_SECRET=your_secret_key_here_at_least_32_chars_long
FRONTEND_URL=http://localhost:3000
"@ | Out-File -FilePath ".env" -Encoding utf8
        Write-Success "Basic .env file created. Please update it with your specific configuration."
    }
}
else {
    Write-Success ".env file already exists."
}

# Restore NuGet packages
Write-Step "Restoring NuGet Packages"
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to restore NuGet packages."
    exit 1
}
Write-Success "NuGet packages restored successfully."

# Build solution
Write-Step "Building Solution"
dotnet build --configuration Debug --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build solution."
    exit 1
}
Write-Success "Solution built successfully."

# Setup LocalDB if needed
if ($SetupLocalDb -or -not $UseDocker) {
    Write-Step "Setting Up SQL Server LocalDB"
    
    # Create LocalDB instance if it doesn't exist
    $localDbInstance = "DecVCPlat"
    $localDbInfo = sqllocaldb info $localDbInstance 2>$null
    
    if ($LASTEXITCODE -ne 0) {
        Write-Success "Creating LocalDB instance: $localDbInstance"
        sqllocaldb create $localDbInstance
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to create LocalDB instance."
            exit 1
        }
    }
    else {
        Write-Success "LocalDB instance already exists: $localDbInstance"
    }
    
    # Start LocalDB instance
    Write-Success "Starting LocalDB instance: $localDbInstance"
    sqllocaldb start $localDbInstance
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to start LocalDB instance."
        exit 1
    }
    
    # Update connection strings in appsettings.json files
    Write-Step "Updating Connection Strings for LocalDB"
    
    $services = @(
        "src\Services\UserManagement\UserManagement.API",
        "src\Services\ProjectManagement\ProjectManagement.API",
        "src\Services\Voting\Voting.API",
        "src\Services\Funding\Funding.API",
        "src\Services\Notification\Notification.API"
    )
    
    foreach ($service in $services) {
        $appSettingsPath = Join-Path $service "appsettings.Development.json"
        
        if (Test-Path $appSettingsPath) {
            Write-Success "Updating connection string in: $appSettingsPath"
            
            $serviceName = Split-Path -Leaf (Split-Path -Parent $service)
            $connectionString = "Server=(localdb)\\DecVCPlat;Database=DecVCPlat_$serviceName;Trusted_Connection=True;MultipleActiveResultSets=true"
            
            $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
            
            # Ensure ConnectionStrings property exists
            if (-not $appSettings.ConnectionStrings) {
                $appSettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
            }
            
            # Update DefaultConnection
            $appSettings.ConnectionStrings.DefaultConnection = $connectionString
            
            $appSettings | ConvertTo-Json -Depth 10 | Out-File $appSettingsPath -Encoding utf8
        }
        else {
            Write-Warning "appsettings.Development.json not found in: $service"
        }
    }
}

# Setup frontend
if ($nodeInstalled -and (Test-Path "src\Frontend")) {
    Write-Step "Setting Up Frontend"
    Set-Location "src\Frontend"
    
    # Install npm packages
    Write-Success "Installing npm packages..."
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to install npm packages. Frontend may not work correctly."
    }
    else {
        Write-Success "npm packages installed successfully."
    }
    
    # Return to root directory
    Set-Location $rootDir
}

# Docker setup
if ($UseDocker) {
    Write-Step "Setting Up Docker Environment"
    
    if ($BuildImages) {
        Write-Success "Building Docker images..."
        docker-compose build
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to build Docker images."
            exit 1
        }
        Write-Success "Docker images built successfully."
    }
    
    if ($StartServices) {
        Write-Success "Starting services with Docker Compose..."
        docker-compose up -d
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to start services with Docker Compose."
            exit 1
        }
        
        Write-Success "Services started successfully."
        Write-Success "`nAccess the services at:"
        Write-Host "- API Gateway: https://localhost:5001" -ForegroundColor White
        Write-Host "- User Management: http://localhost:5010" -ForegroundColor White
        Write-Host "- Project Management: http://localhost:5020" -ForegroundColor White
        Write-Host "- Voting: http://localhost:5030" -ForegroundColor White
        Write-Host "- Funding: http://localhost:5040" -ForegroundColor White
        Write-Host "- Notification: http://localhost:5050" -ForegroundColor White
        Write-Host "- Frontend: http://localhost:3000" -ForegroundColor White
    }
}

# Final message
Write-Step "Setup Complete"
Write-Success "DecVCPlat development environment setup is complete!"

if ($UseDocker) {
    Write-Host "`nTo start the services:" -ForegroundColor Cyan
    Write-Host "cd scripts" -ForegroundColor White
    Write-Host ".\run-docker-services.ps1 -DetachedMode" -ForegroundColor White
    
    Write-Host "`nTo view logs for a specific service:" -ForegroundColor Cyan
    Write-Host "docker-compose logs -f [service-name]" -ForegroundColor White
    
    Write-Host "`nTo stop the services:" -ForegroundColor Cyan
    Write-Host "docker-compose down" -ForegroundColor White
}
else {
    Write-Host "`nTo start the backend services, open a separate terminal for each service and run:" -ForegroundColor Cyan
    Write-Host "cd src\Services\{ServiceName}\{ServiceName}.API" -ForegroundColor White
    Write-Host "dotnet run" -ForegroundColor White
    
    Write-Host "`nTo start the frontend:" -ForegroundColor Cyan
    Write-Host "cd src\Frontend" -ForegroundColor White
    Write-Host "npm start" -ForegroundColor White
}

Write-Host "`nFor detailed instructions, see DEV_SETUP.md" -ForegroundColor Cyan

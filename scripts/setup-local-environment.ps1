# DecVCPlat Local Environment Setup Script
# This script sets up the local development environment for DecVCPlat without Docker

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

# Check prerequisites
Write-Step "Checking Prerequisites"

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version
    Write-Success ".NET SDK is installed (Version: $dotnetVersion)."
}
catch {
    Write-Error ".NET SDK is not installed or not in PATH. Please install .NET SDK to continue."
    Write-Error "Download from: https://dotnet.microsoft.com/download"
    exit 1
}

# Check if Node.js is installed
$nodeInstalled = $false
try {
    $nodeVersion = node --version
    Write-Success "Node.js is installed (Version: $nodeVersion)."
    $nodeInstalled = $true
}
catch {
    Write-Warning "Node.js is not installed or not in PATH. Frontend development will be limited."
    Write-Warning "Download from: https://nodejs.org/"
}

# Check if SQL Server LocalDB is installed
$localDbInstalled = $false
try {
    sqllocaldb info
    if ($LASTEXITCODE -eq 0) {
        Write-Success "SQL Server LocalDB is installed."
        $localDbInstalled = $true
    }
}
catch {
    Write-Warning "SQL Server LocalDB is not installed or not in PATH. It's required for local database development."
    Write-Warning "Install SQL Server Express with LocalDB: https://www.microsoft.com/en-us/sql-server/sql-server-downloads"
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

# Find solution file
Write-Step "Finding Solution File"
$solutionFiles = Get-ChildItem -Path $rootDir -Filter "*.sln" -Recurse -Depth 2

if ($solutionFiles.Count -eq 0) {
    Write-Warning "No solution file found. Skipping NuGet restore and build."
    $solutionFile = $null
}
elseif ($solutionFiles.Count -eq 1) {
    $solutionFile = $solutionFiles[0].FullName
    Write-Success "Found solution file: $solutionFile"
}
else {
    # Multiple solution files found, use the one in src directory if available
    $srcSolutionFile = $solutionFiles | Where-Object { $_.FullName -like "*\src\*.sln" } | Select-Object -First 1
    
    if ($srcSolutionFile) {
        $solutionFile = $srcSolutionFile.FullName
        Write-Success "Using solution file from src directory: $solutionFile"
    }
    else {
        # Use the first solution file found
        $solutionFile = $solutionFiles[0].FullName
        Write-Success "Multiple solution files found. Using: $solutionFile"
    }
}

# Restore NuGet packages if solution file exists
if ($solutionFile) {
    Write-Step "Restoring NuGet Packages"
    dotnet restore $solutionFile
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to restore NuGet packages. Continuing anyway."
    }
    else {
        Write-Success "NuGet packages restored successfully."
    }

    # Build solution
    Write-Step "Building Solution"
    dotnet build $solutionFile --configuration Debug --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to build solution. Continuing anyway."
    }
    else {
        Write-Success "Solution built successfully."
    }
}
else {
    Write-Warning "Skipping NuGet restore and build as no solution file was found."
}

# Setup LocalDB
if ($localDbInstalled) {
    Write-Step "Setting Up SQL Server LocalDB"
    
    # Create LocalDB instance if it doesn't exist
    $localDbInstance = "DecVCPlat"
    sqllocaldb info $localDbInstance 2>$null
    
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
            
            try {
                $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
                
                # Ensure ConnectionStrings property exists
                if (-not $appSettings.ConnectionStrings) {
                    $appSettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
                }
                
                # Update DefaultConnection
                $appSettings.ConnectionStrings.DefaultConnection = $connectionString
                
                $appSettings | ConvertTo-Json -Depth 10 | Out-File $appSettingsPath -Encoding utf8
            }
            catch {
                Write-Warning "Failed to update connection string in: $appSettingsPath"
                Write-Warning "Please update the connection string manually."
            }
        }
        else {
            Write-Warning "appsettings.Development.json not found in: $service"
            
            # Create a basic appsettings.Development.json file
            $serviceName = Split-Path -Leaf (Split-Path -Parent $service)
            $connectionString = "Server=(localdb)\\DecVCPlat;Database=DecVCPlat_$serviceName;Trusted_Connection=True;MultipleActiveResultSets=true"
            
            $appSettingsObject = [ordered]@{}
            $appSettingsObject.Add("Logging", @{})
            $appSettingsObject.Logging.Add("LogLevel", @{})
            $appSettingsObject.Logging.LogLevel.Add("Default", "Information")
            $appSettingsObject.Logging.LogLevel.Add("Microsoft", "Warning")
            $appSettingsObject.Logging.LogLevel.Add("Microsoft.Hosting.Lifetime", "Information")
            $appSettingsObject.Add("ConnectionStrings", @{})
            $appSettingsObject.ConnectionStrings.Add("DefaultConnection", $connectionString)
            $appSettingsObject.Add("AllowedHosts", "*")
            
            $serviceDir = Join-Path $rootDir $service
            if (Test-Path $serviceDir) {
                $appSettingsObject | ConvertTo-Json -Depth 10 | Out-File (Join-Path $serviceDir "appsettings.Development.json") -Encoding utf8
                Write-Success "Created appsettings.Development.json in: $service"
            }
        }
    }
    
    # Apply migrations
    Write-Step "Applying Database Migrations"
    Write-Warning "To apply migrations for each service, run the following commands manually:"
    
    foreach ($service in $services) {
        $serviceName = Split-Path -Leaf (Split-Path -Parent $service)
        Write-Host "cd $service" -ForegroundColor White
        Write-Host "dotnet ef database update" -ForegroundColor White
        Write-Host ""
    }
}
else {
    Write-Warning "Skipping LocalDB setup as it's not installed."
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
else {
    Write-Warning "Skipping frontend setup as Node.js is not installed or frontend directory not found."
}

# Final message
Write-Step "Setup Complete"
Write-Success "DecVCPlat local development environment setup is complete!"

Write-Host "`nTo start the backend services, open a separate terminal for each service and run:" -ForegroundColor Cyan
Write-Host "cd src\Services\{ServiceName}\{ServiceName}.API" -ForegroundColor White
Write-Host "dotnet run" -ForegroundColor White

if ($nodeInstalled) {
    Write-Host "`nTo start the frontend:" -ForegroundColor Cyan
    Write-Host "cd src\Frontend" -ForegroundColor White
    Write-Host "npm start" -ForegroundColor White
}

Write-Host "`nFor detailed instructions, see DEV_SETUP.md" -ForegroundColor Cyan

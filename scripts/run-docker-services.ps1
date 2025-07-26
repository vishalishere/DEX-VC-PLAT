# Run Docker Services Script for DecVCPlat
# This script helps run the DecVCPlat services using Docker Compose

param (
    [Parameter(Mandatory=$false)]
    [switch]$BuildImages,
    
    [Parameter(Mandatory=$false)]
    [switch]$DetachedMode,
    
    [Parameter(Mandatory=$false)]
    [string]$Service
)

# Ensure the script stops on first error
$ErrorActionPreference = "Stop"

# Check if Docker is installed
try {
    docker --version
    Write-Host "Docker is installed." -ForegroundColor Green
}
catch {
    Write-Host "Docker is not installed or not in PATH. Please install Docker to continue." -ForegroundColor Red
    exit 1
}

# Check if Docker Compose is installed
try {
    docker-compose --version
    Write-Host "Docker Compose is installed." -ForegroundColor Green
}
catch {
    Write-Host "Docker Compose is not installed or not in PATH. Please install Docker Compose to continue." -ForegroundColor Red
    exit 1
}

# Navigate to the root directory
Set-Location ..

# Build Docker images if requested
if ($BuildImages) {
    Write-Host "Building Docker images..." -ForegroundColor Green
    
    if ($Service) {
        docker-compose build $Service
    } else {
        docker-compose build
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to build Docker images." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Docker images built successfully." -ForegroundColor Green
}

# Run Docker Compose
Write-Host "Starting services with Docker Compose..." -ForegroundColor Green

$composeCommand = "docker-compose up"

if ($DetachedMode) {
    $composeCommand += " -d"
}

if ($Service) {
    $composeCommand += " $Service"
}

Write-Host "Running command: $composeCommand" -ForegroundColor Yellow
Invoke-Expression $composeCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start services with Docker Compose." -ForegroundColor Red
    exit 1
}

# If running in detached mode, show the running containers
if ($DetachedMode) {
    Write-Host "`nServices started in detached mode. Running containers:" -ForegroundColor Green
    docker-compose ps
    
    Write-Host "`nAccess the services at:" -ForegroundColor Cyan
    Write-Host "- API Gateway: https://localhost:5001" -ForegroundColor White
    Write-Host "- User Management: http://localhost:5010" -ForegroundColor White
    Write-Host "- Project Management: http://localhost:5020" -ForegroundColor White
    Write-Host "- Voting: http://localhost:5030" -ForegroundColor White
    Write-Host "- Funding: http://localhost:5040" -ForegroundColor White
    Write-Host "- Notification: http://localhost:5050" -ForegroundColor White
    Write-Host "- Frontend: http://localhost:3000" -ForegroundColor White
    
    Write-Host "`nTo view logs for a specific service:" -ForegroundColor Cyan
    Write-Host "docker-compose logs -f [service-name]" -ForegroundColor White
    
    Write-Host "`nTo stop the services:" -ForegroundColor Cyan
    Write-Host "docker-compose down" -ForegroundColor White
}

Write-Host "`nHappy coding!" -ForegroundColor Green

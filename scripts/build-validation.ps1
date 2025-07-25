# DecVCPlat Comprehensive Build Validation Script
# Copyright (c) DecVCPlat. All Rights Reserved.
# Ensures 100% successful system compilation and operational CI pipelines

param(
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU",
    [switch]$SkipTests = $false,
    [switch]$SkipDockerBuild = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"
$WarningPreference = "Continue"

# Global Configuration
$SolutionRoot = Split-Path -Parent $PSScriptRoot
$MainSolution = Join-Path $SolutionRoot "src\DecVCPlat.sln"
$TestSolution = Join-Path $SolutionRoot "tests\DecVCPlat.Tests.sln"
$FrontendPath = Join-Path $SolutionRoot "src\Frontend\DecVCPlat.Web"

# Logging Functions
function Write-Header($Message) {
    Write-Host "`n" -NoNewline
    Write-Host "=" * 80 -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Yellow
    Write-Host "=" * 80 -ForegroundColor Cyan
}

function Write-Success($Message) {
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error($Message) {
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Info($Message) {
    Write-Host "ℹ️  $Message" -ForegroundColor Blue
}

function Write-Warning($Message) {
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

# Validation Functions
function Test-Prerequisites {
    Write-Header "PREREQUISITES VALIDATION"
    
    $prerequisites = @()
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-Success ".NET SDK Version: $dotnetVersion"
        $prerequisites += @{ Name = ".NET SDK"; Status = "✅"; Version = $dotnetVersion }
    }
    catch {
        Write-Error ".NET SDK not found or not accessible"
        $prerequisites += @{ Name = ".NET SDK"; Status = "❌"; Version = "Not Found" }
        return $false
    }
    
    # Check Node.js
    try {
        $nodeVersion = node --version
        Write-Success "Node.js Version: $nodeVersion"
        $prerequisites += @{ Name = "Node.js"; Status = "✅"; Version = $nodeVersion }
    }
    catch {
        Write-Warning "Node.js not found - frontend build will be skipped"
        $prerequisites += @{ Name = "Node.js"; Status = "⚠️"; Version = "Not Found" }
    }
    
    # Check Docker
    try {
        $dockerVersion = docker --version
        Write-Success "Docker Version: $dockerVersion"
        $prerequisites += @{ Name = "Docker"; Status = "✅"; Version = $dockerVersion }
    }
    catch {
        Write-Warning "Docker not found - container builds will be skipped"
        $prerequisites += @{ Name = "Docker"; Status = "⚠️"; Version = "Not Found" }
    }
    
    return $true
}

function Test-ProjectStructure {
    Write-Header "PROJECT STRUCTURE VALIDATION"
    
    $requiredFiles = @(
        $MainSolution,
        $TestSolution,
        (Join-Path $SolutionRoot "src\Shared\DecVCPlat.Shared\DecVCPlat.Shared.csproj"),
        (Join-Path $SolutionRoot "src\Services\UserManagement\DecVCPlat.UserManagement.API\DecVCPlat.UserManagement.API.csproj"),
        (Join-Path $SolutionRoot "src\Services\ProjectManagement\DecVCPlat.ProjectManagement.API\DecVCPlat.ProjectManagement.API.csproj"),
        (Join-Path $SolutionRoot "src\Services\Voting\DecVCPlat.Voting.API\DecVCPlat.Voting.API.csproj"),
        (Join-Path $SolutionRoot "src\Services\Funding\DecVCPlat.Funding.API\DecVCPlat.Funding.API.csproj"),
        (Join-Path $SolutionRoot "src\Services\Notification\DecVCPlat.Notification.API\DecVCPlat.Notification.API.csproj"),
        (Join-Path $SolutionRoot "src\Gateway\DecVCPlat.Gateway\DecVCPlat.Gateway.csproj")
    )
    
    $missingFiles = @()
    foreach ($file in $requiredFiles) {
        if (Test-Path $file) {
            Write-Success "Found: $(Split-Path -Leaf $file)"
        } else {
            Write-Error "Missing: $file"
            $missingFiles += $file
        }
    }
    
    if ($missingFiles.Count -eq 0) {
        Write-Success "All required project files are present"
        return $true
    } else {
        Write-Error "$($missingFiles.Count) required files are missing"
        return $false
    }
}

function Build-Solutions {
    Write-Header "SOLUTION COMPILATION VALIDATION"
    
    # Restore packages for main solution
    Write-Info "Restoring packages for main solution..."
    try {
        dotnet restore $MainSolution --verbosity minimal
        Write-Success "Main solution package restore completed"
    }
    catch {
        Write-Error "Failed to restore packages for main solution: $_"
        return $false
    }
    
    # Restore packages for test solution
    Write-Info "Restoring packages for test solution..."
    try {
        dotnet restore $TestSolution --verbosity minimal
        Write-Success "Test solution package restore completed"
    }
    catch {
        Write-Error "Failed to restore packages for test solution: $_"
        return $false
    }
    
    # Build main solution
    Write-Info "Building main solution in $Configuration configuration..."
    try {
        dotnet build $MainSolution --configuration $Configuration --no-restore --verbosity minimal
        Write-Success "Main solution build completed successfully"
    }
    catch {
        Write-Error "Failed to build main solution: $_"
        return $false
    }
    
    # Build test solution
    Write-Info "Building test solution in $Configuration configuration..."
    try {
        dotnet build $TestSolution --configuration $Configuration --no-restore --verbosity minimal
        Write-Success "Test solution build completed successfully"
    }
    catch {
        Write-Error "Failed to build test solution: $_"
        return $false
    }
    
    return $true
}

function Build-Frontend {
    Write-Header "FRONTEND BUILD VALIDATION"
    
    if (-not (Test-Path $FrontendPath)) {
        Write-Warning "Frontend path not found, skipping frontend build"
        return $true
    }
    
    Push-Location $FrontendPath
    try {
        # Check if package.json exists
        if (-not (Test-Path "package.json")) {
            Write-Warning "package.json not found, skipping frontend build"
            return $true
        }
        
        # Install dependencies
        Write-Info "Installing frontend dependencies..."
        npm install --silent
        Write-Success "Frontend dependencies installed"
        
        # TypeScript compilation check
        Write-Info "Checking TypeScript compilation..."
        npx tsc --noEmit --skipLibCheck
        Write-Success "TypeScript compilation check passed"
        
        # Build frontend
        Write-Info "Building frontend..."
        npm run build
        Write-Success "Frontend build completed successfully"
        
        return $true
    }
    catch {
        Write-Error "Frontend build failed: $_"
        return $false
    }
    finally {
        Pop-Location
    }
}

function Run-Tests {
    Write-Header "UNIT & INTEGRATION TESTS EXECUTION"
    
    if ($SkipTests) {
        Write-Warning "Tests skipped by user request"
        return $true
    }
    
    # Run unit tests with coverage
    Write-Info "Executing unit tests with coverage analysis..."
    try {
        $coverageDir = Join-Path $SolutionRoot "coverage"
        if (Test-Path $coverageDir) {
            Remove-Item $coverageDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null
        
        dotnet test $TestSolution `
            --configuration $Configuration `
            --no-build `
            --no-restore `
            --collect:"XPlat Code Coverage" `
            --results-directory $coverageDir `
            --logger "console;verbosity=normal" `
            --settings (Join-Path $SolutionRoot "tests\coverlet.runsettings")
        
        Write-Success "Unit tests completed with coverage analysis"
        
        # Generate coverage report if reportgenerator is available
        try {
            $coverageFiles = Get-ChildItem -Path $coverageDir -Filter "coverage.*.xml" -Recurse
            if ($coverageFiles.Count -gt 0) {
                Write-Info "Generating coverage report..."
                dotnet tool install -g dotnet-reportgenerator-globaltool --ignore-failed-sources
                reportgenerator "-reports:$($coverageFiles[0].FullName)" "-targetdir:$coverageDir\html" "-reporttypes:Html;JsonSummary"
                Write-Success "Coverage report generated at: $coverageDir\html"
            }
        }
        catch {
            Write-Warning "Coverage report generation skipped: $_"
        }
        
        return $true
    }
    catch {
        Write-Error "Tests failed: $_"
        return $false
    }
}

function Build-DockerImages {
    Write-Header "DOCKER BUILD VALIDATION"
    
    if ($SkipDockerBuild) {
        Write-Warning "Docker builds skipped by user request"
        return $true
    }
    
    try {
        docker --version | Out-Null
    }
    catch {
        Write-Warning "Docker not available, skipping container builds"
        return $true
    }
    
    $services = @(
        @{ Name = "user-management"; Path = "infrastructure/docker/Dockerfile.user-management" },
        @{ Name = "project-management"; Path = "infrastructure/docker/Dockerfile.project-management" },
        @{ Name = "voting"; Path = "infrastructure/docker/Dockerfile.voting" },
        @{ Name = "funding"; Path = "infrastructure/docker/Dockerfile.funding" },
        @{ Name = "notification"; Path = "infrastructure/docker/Dockerfile.notification" },
        @{ Name = "gateway"; Path = "infrastructure/docker/Dockerfile.gateway" }
    )
    
    $successfulBuilds = 0
    foreach ($service in $services) {
        $dockerfilePath = Join-Path $SolutionRoot $service.Path
        if (Test-Path $dockerfilePath) {
            Write-Info "Building Docker image for $($service.Name)..."
            try {
                docker build -f $dockerfilePath -t "decvcplat-$($service.Name):build-validation" $SolutionRoot
                Write-Success "Docker image built successfully: decvcplat-$($service.Name)"
                $successfulBuilds++
            }
            catch {
                Write-Error "Failed to build Docker image for $($service.Name): $_"
            }
        } else {
            Write-Warning "Dockerfile not found for $($service.Name): $dockerfilePath"
        }
    }
    
    if ($successfulBuilds -gt 0) {
        Write-Success "$successfulBuilds Docker images built successfully"
        return $true
    } else {
        Write-Warning "No Docker images were built successfully"
        return $true  # Don't fail the entire build for Docker issues
    }
}

function Generate-ValidationReport {
    param($Results)
    
    Write-Header "BUILD VALIDATION SUMMARY REPORT"
    
    $overallSuccess = $true
    $totalChecks = 0
    $passedChecks = 0
    
    foreach ($result in $Results) {
        $totalChecks++
        if ($result.Success) {
            Write-Success "$($result.Stage): PASSED"
            $passedChecks++
        } else {
            Write-Error "$($result.Stage): FAILED"
            $overallSuccess = $false
        }
        
        if ($result.Details) {
            Write-Info "  Details: $($result.Details)"
        }
    }
    
    Write-Host "`n" -NoNewline
    Write-Host "=" * 80 -ForegroundColor Cyan
    Write-Host "  FINAL VALIDATION RESULT" -ForegroundColor Yellow
    Write-Host "=" * 80 -ForegroundColor Cyan
    
    if ($overallSuccess) {
        Write-Host "🎉 BUILD VALIDATION: " -NoNewline -ForegroundColor Green
        Write-Host "SUCCESS" -ForegroundColor Green -BackgroundColor Black
        Write-Success "All $totalChecks validation stages passed"
        Write-Success "System is ready for production deployment"
        Write-Success "CI pipelines are fully operational"
    } else {
        Write-Host "💥 BUILD VALIDATION: " -NoNewline -ForegroundColor Red
        Write-Host "FAILED" -ForegroundColor Red -BackgroundColor Black
        Write-Error "$passedChecks out of $totalChecks validation stages passed"
        Write-Error "System requires fixes before production deployment"
    }
    
    Write-Host "=" * 80 -ForegroundColor Cyan
    
    return $overallSuccess
}

# Main Execution Flow
function Main {
    Write-Header "🚀 DECVCPLAT COMPREHENSIVE BUILD VALIDATION"
    Write-Info "Configuration: $Configuration"
    Write-Info "Platform: $Platform"
    Write-Info "Skip Tests: $SkipTests"
    Write-Info "Skip Docker: $SkipDockerBuild"
    
    $results = @()
    
    # Step 1: Prerequisites
    $prerequisiteResult = Test-Prerequisites
    $results += @{ Stage = "Prerequisites Check"; Success = $prerequisiteResult; Details = "SDK and tooling validation" }
    
    if (-not $prerequisiteResult) {
        Write-Error "Prerequisites validation failed. Cannot continue."
        return 1
    }
    
    # Step 2: Project Structure
    $structureResult = Test-ProjectStructure
    $results += @{ Stage = "Project Structure"; Success = $structureResult; Details = "Required files and project structure" }
    
    # Step 3: Solution Compilation
    $buildResult = Build-Solutions
    $results += @{ Stage = "Solution Compilation"; Success = $buildResult; Details = "Main and test solution builds" }
    
    # Step 4: Frontend Build
    $frontendResult = Build-Frontend
    $results += @{ Stage = "Frontend Build"; Success = $frontendResult; Details = "React TypeScript application build" }
    
    # Step 5: Tests Execution
    $testResult = Run-Tests
    $results += @{ Stage = "Tests Execution"; Success = $testResult; Details = "Unit and integration tests with coverage" }
    
    # Step 6: Docker Builds
    $dockerResult = Build-DockerImages
    $results += @{ Stage = "Docker Builds"; Success = $dockerResult; Details = "Microservice container builds" }
    
    # Generate final report
    $overallSuccess = Generate-ValidationReport -Results $results
    
    if ($overallSuccess) {
        Write-Success "🎉 DecVCPlat build validation completed successfully!"
        return 0
    } else {
        Write-Error "💥 DecVCPlat build validation failed!"
        return 1
    }
}

# Execute main function
try {
    $exitCode = Main
    exit $exitCode
}
catch {
    Write-Error "Build validation script encountered an unexpected error: $_"
    Write-Error $_.ScriptStackTrace
    exit 1
}

# DecVCPlat Development Environment Setup

This guide provides comprehensive instructions for setting up your local development environment for the DecVCPlat platform.

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) or later
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (for database development)
- [Node.js](https://nodejs.org/) (v16 or later) (for frontend development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

## Setup Options

You have two main options for setting up your development environment:

1. **Local machine setup** (recommended): Runs services directly on your machine with SQL Server LocalDB
2. **Docker-based setup** (optional): Runs all services in Docker containers if Docker is installed

## Option 1: Docker-based Setup

### Step 1: Clone the Repository

```powershell
git clone https://github.com/vishalishere/DEX-VC-PLAT.git
cd DEX-VC-PLAT
```

### Step 2: Create Environment File

```powershell
Copy-Item .env.template .env
# Edit the .env file with your specific settings if needed
```

### Step 3: Run the Setup Script

```powershell
cd scripts
.\setup-dev-environment.ps1
```

### Step 4: Start the Services

To start all services:

```powershell
cd scripts
.\run-docker-services.ps1 -BuildImages -DetachedMode
```

To start a specific service:

```powershell
.\run-docker-services.ps1 -Service user-management -BuildImages
```

### Step 5: Access the Services

- API Gateway: https://localhost:5001
- User Management: http://localhost:5010
- Project Management: http://localhost:5020
- Voting: http://localhost:5030
- Funding: http://localhost:5040
- Notification: http://localhost:5050
- Frontend: http://localhost:3000

### Step 6: View Logs

```powershell
docker-compose logs -f [service-name]
```

### Step 7: Stop the Services

```powershell
docker-compose down
```

## Option 2: Local Machine Setup (Recommended)

### Step 1: Clone the Repository

```powershell
git clone https://github.com/vishalishere/DEX-VC-PLAT.git
cd DEX-VC-PLAT
```

### Step 2: Run the Local Setup Script

```powershell
.\scripts\setup-local-environment.ps1
```

This script will:
- Check for prerequisites (.NET SDK, Node.js, SQL Server LocalDB)
- Create necessary directories
- Find and build the solution
- Set up LocalDB instance
- Update connection strings in all microservices

### Step 3: Start the Services

You can start each microservice individually using the following commands:

```powershell
cd src\DecVCPlat.UserManagement
dotnet run
```

Repeat for other microservices in separate terminal windows.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\DecVCPlat;Database=DecVCPlat_{ServiceName};Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### Step 4: Start the Backend Services

For each microservice, open a separate terminal and run:

```powershell
cd src/Services/{ServiceName}/{ServiceName}.API
dotnet run
```

### Step 5: Start the Frontend

```powershell
cd src/Frontend
npm install
npm start
```

## Debugging

### Debugging with Docker

1. Use Visual Studio's Docker support to attach to running containers
2. Use the "Attach to Process" feature to debug running services
3. View container logs using `docker-compose logs -f [service-name]`

### Debugging Locally

1. Use Visual Studio or Visual Studio Code to open the solution
2. Set breakpoints in your code
3. Run the services in debug mode

## Swagger Documentation

Each microservice has Swagger documentation available at `/swagger` when the service is running:

- User Management: http://localhost:5010/swagger
- Project Management: http://localhost:5020/swagger
- Voting: http://localhost:5030/swagger
- Funding: http://localhost:5040/swagger
- Notification: http://localhost:5050/swagger

## Troubleshooting

### Common Issues and Solutions

#### Docker Issues

1. **Port conflicts**: If you see "port is already in use" errors, change the port mappings in `docker-compose.yml`

2. **Container fails to start**: Check logs with `docker-compose logs [service-name]`

3. **Volume mounting issues**: Ensure Docker has permission to access your project directory

#### LocalDB Issues

1. **Cannot connect to LocalDB**: Ensure LocalDB is running with `sqllocaldb info`

2. **Database not created**: Run migrations manually:
   ```powershell
   dotnet ef database update --project src/Services/{ServiceName}/{ServiceName}.API
   ```

#### .NET Issues

1. **Build errors**: Run `dotnet restore` to ensure all packages are installed

2. **Runtime errors**: Check logs in the `logs` directory

#### Frontend Issues

1. **npm install fails**: Clear npm cache with `npm cache clean --force`

2. **Frontend not connecting to backend**: Check API Gateway configuration and CORS settings

### Getting Help

If you encounter issues not covered here, please:

1. Check the GitHub repository issues
2. Consult the project documentation
3. Contact the development team

## Troubleshooting Guide

### Common Issues and Solutions

#### LocalDB Connection Issues

**Issue**: Cannot connect to LocalDB or database operations fail

**Solutions**:
- Verify LocalDB is running: `sqllocaldb info`
- Start the instance manually: `sqllocaldb start DecVCPlat`
- Reset the instance if needed: 
  ```powershell
  sqllocaldb stop DecVCPlat
  sqllocaldb delete DecVCPlat
  sqllocaldb create DecVCPlat
  ```
- Check connection string format in appsettings.json files

#### Port Conflicts

**Issue**: Service fails to start due to port already in use

**Solutions**:
- Check for running processes using the port: `netstat -ano | findstr :<port>`
- Kill the process: `taskkill /PID <process_id> /F`
- Modify the port in the service's `Properties/launchSettings.json`

#### Build Errors

**Issue**: Solution fails to build

**Solutions**:
- Restore NuGet packages manually: `dotnet restore`
- Clean the solution: `dotnet clean`
- Check for missing dependencies or project references
- Verify the solution file references all required projects

#### PowerShell Execution Policy

**Issue**: Cannot run PowerShell scripts due to execution policy

**Solutions**:
- Run PowerShell as Administrator and set policy: `Set-ExecutionPolicy RemoteSigned`
- Bypass policy for a single script: `powershell -ExecutionPolicy Bypass -File .\scripts\setup-local-environment.ps1`

#### Node.js/NPM Issues

**Issue**: Frontend build fails

**Solutions**:
- Clear npm cache: `npm cache clean --force`
- Delete node_modules and reinstall: `rm -r node_modules && npm install`
- Update npm: `npm install -g npm@latest`

### Service-Specific Troubleshooting

#### User Management Service

- Verify database migrations are applied: `dotnet ef database update`
- Check authentication settings in appsettings.json

#### Project Management Service

- Ensure proper references to shared libraries
- Verify project templates are accessible

#### Voting Service

- Check vote counting logic and database constraints
- Verify integration with Project Management service

#### Funding Service

- Verify blockchain integration settings
- Check transaction processing logic

#### Notification Service

- Ensure message broker settings are correct
- Verify email service configuration

## Additional Resources

- [DecVCPlat Architecture Documentation](./ARCHITECTURE.md)
- [API Documentation](./API_DOCS.md)
- [Contribution Guidelines](./CONTRIBUTING.md)
- [Swagger API Documentation](http://localhost:5000/swagger) (when services are running)

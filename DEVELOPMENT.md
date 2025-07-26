# DecVCPlat Development Workflow

This document outlines the development workflow and best practices for working with the DecVCPlat project.

## Table of Contents
- [Development Environment Setup](#development-environment-setup)
- [Project Structure](#project-structure)
- [Branching Strategy](#branching-strategy)
- [Development Workflow](#development-workflow)
- [Testing](#testing)
- [API Documentation](#api-documentation)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

## Development Environment Setup

### Prerequisites
- .NET 8.0 SDK
- SQL Server LocalDB
- Node.js 18+ (for frontend development)
- Git
- Visual Studio 2022 or Visual Studio Code

### Initial Setup

1. **Clone the repository**
   ```
   git clone https://github.com/vishalishere/DEX-VC-PLAT.git
   cd DEX-VC-PLAT
   ```

2. **Setup the development environment**
   - Run the setup script from the `src` directory:
   ```
   cd src
   .\Setup-Complete.ps1
   ```
   - This script will:
     - Restore NuGet packages
     - Build the solution
     - Setup the LocalDB databases
     - Apply migrations

3. **Configure connection strings**
   - The connection strings in `appsettings.json` for each microservice should be configured to use SQL Server LocalDB
   - Example connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DecVCPlat_ServiceName;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. **Frontend setup**
   ```
   cd src/Frontend
   npm install
   ```

## Project Structure

The DecVCPlat project follows a microservices architecture:

```
DecVCPlat/
├── src/
│   ├── Backend/
│   │   ├── DecVCPlat.ApiGateway/       # API Gateway service
│   │   ├── DecVCPlat.UserManagement/   # User management microservice
│   │   ├── DecVCPlat.ProjectManagement/# Project management microservice
│   │   ├── DecVCPlat.Voting/           # Voting microservice
│   │   ├── DecVCPlat.Funding/          # Funding microservice
│   │   ├── DecVCPlat.Notification/     # Notification microservice
│   │   └── DecVCPlat.Shared/           # Shared libraries and utilities
│   ├── Frontend/                       # React frontend application
│   └── DecVCPlat.sln                   # Solution file
├── tests/                              # Test projects
├── docs/                               # Documentation
├── scripts/                            # Utility scripts
└── README.md                           # Project overview
```

## Branching Strategy

We follow a Git Flow branching strategy:

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches
- `release/*` - Release preparation branches
- `hotfix/*` - Urgent fixes for production

### Branch Naming Convention

- Feature branches: `feature/feature-name`
- Bug fix branches: `bugfix/bug-description`
- Release branches: `release/version-number`
- Hotfix branches: `hotfix/issue-description`

## Development Workflow

1. **Create a new feature branch**
   ```
   git checkout develop
   git pull
   git checkout -b feature/your-feature-name
   ```

2. **Implement your changes**
   - Follow the coding standards and best practices
   - Keep commits small and focused
   - Write meaningful commit messages

3. **Run local tests**
   - Ensure all tests pass before submitting a pull request
   - Add new tests for new functionality

4. **Create a pull request**
   - Push your branch to GitHub
   - Create a pull request to merge into the `develop` branch
   - Fill in the pull request template with details about your changes

5. **Code review**
   - Address feedback from code reviews
   - Make necessary changes and push updates

6. **Merge**
   - Once approved, your pull request will be merged into the `develop` branch
   - Delete your feature branch after merging

## Testing

### Types of Tests

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test interactions between components
- **API Tests**: Test API endpoints
- **UI Tests**: Test frontend components and user flows

### Running Tests

- **Backend Tests**
  ```
  dotnet test
  ```

- **Frontend Tests**
  ```
  cd src/Frontend
  npm test
  ```

## API Documentation

Each microservice includes Swagger/OpenAPI documentation:

- Access Swagger UI by running the service and navigating to the root URL
- Example: `https://localhost:5001/` for a service running on port 5001
- The API documentation provides details on all available endpoints, request/response models, and authentication requirements

### Generating API Client Libraries

You can generate client libraries for the APIs using the Swagger/OpenAPI specifications:

```
swagger-codegen generate -i https://localhost:5001/swagger/v1/swagger.json -l csharp -o ./client
```

## Deployment

### Local Deployment

For local development, you can run the services individually or use Docker Compose:

- **Running individual services**
  ```
  cd src/Backend/DecVCPlat.ServiceName
  dotnet run
  ```

- **Using Docker Compose**
  ```
  docker-compose up
  ```

### CI/CD Pipeline

The project uses GitHub Actions for continuous integration and deployment:

1. **Build and Test**: Triggered on every push and pull request
2. **Deploy to Dev**: Automatically deployed to development environment when merged to `develop`
3. **Deploy to Staging**: Manually triggered deployment to staging environment
4. **Deploy to Production**: Manually triggered deployment to production environment after approval

## Troubleshooting

### Common Issues

1. **Database Connection Issues**
   - Ensure SQL Server LocalDB is running
   - Verify connection strings in `appsettings.json`
   - Run `sqllocaldb info` to check available LocalDB instances

2. **Build Errors**
   - Ensure you have the correct .NET SDK version installed
   - Run `dotnet restore` to restore NuGet packages
   - Check for syntax errors or missing references

3. **Runtime Errors**
   - Check application logs in the `logs` directory
   - Verify environment variables are set correctly
   - Ensure all required services are running

### Getting Help

- Check existing issues on GitHub
- Create a new issue with detailed information about the problem
- Reach out to the development team on the project's communication channels

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [React Documentation](https://reactjs.org/docs/getting-started.html)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Swagger/OpenAPI Documentation](https://swagger.io/docs/)

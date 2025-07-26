# DecVCPlat - Decentralized Venture Capital Platform

![DecVCPlat Logo](https://via.placeholder.com/150x150.png?text=DecVCPlat)

## Overview

DecVCPlat is an enterprise-grade decentralized venture capital platform built with a microservices architecture. It enables transparent, efficient, and secure venture capital operations using blockchain technology, smart contracts, and modern web technologies.

## Features

- **User Management**: Secure authentication, authorization, and profile management
- **Project Management**: Submit, review, and track investment proposals
- **Voting System**: Transparent decision-making through secure voting mechanisms
- **Funding Management**: Automated fund distribution and tracking
- **Notifications**: Real-time alerts and updates for platform activities
- **Blockchain Integration**: Smart contract execution for funding and voting processes
- **Analytics Dashboard**: Comprehensive insights into platform performance

## Architecture

DecVCPlat follows a microservices architecture with the following components:

- **Frontend**: React-based single-page application
- **Backend Services**:
  - User Management Service
  - Project Management Service
  - Voting Service
  - Funding Service
  - Notification Service
- **Infrastructure**:
  - SQL Server for data persistence
  - Redis for caching
  - RabbitMQ for messaging
  - Docker for containerization
  - Azure for cloud hosting
  - Blockchain integration for smart contracts

## Technology Stack

- **Frontend**: React, Redux, TypeScript, Material-UI
- **Backend**: .NET 8.0, ASP.NET Core, Entity Framework Core
- **Database**: SQL Server, LocalDB (for development)
- **Authentication**: JWT, OAuth 2.0
- **DevOps**: Docker, GitHub Actions, Azure DevOps
- **Blockchain**: Ethereum, Solidity Smart Contracts
- **Testing**: xUnit, Jest, Cypress

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Docker](https://www.docker.com/products/docker-desktop/) (optional)
- [Git](https://git-scm.com/downloads)

### Local Development Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/vishalishere/DEX-VC-PLAT.git
   cd DEX-VC-PLAT
   ```

2. **Run the setup script**

   ```powershell
   cd src
   .\Setup-Final.ps1
   ```

   This script will:
   - Check for prerequisites
   - Restore NuGet packages
   - Build the solution
   - Set up the frontend development environment

## Documentation

See the [docs](./docs) directory for detailed documentation on:

- API Reference
- Deployment Guide
- Development Workflow
- Architecture Overview
- [Blockchain Integration](./docs/BLOCKCHAIN.md)
- [Analytics System](./docs/ANALYTICS.md)
- [Security Features](./docs/SECURITY.md)
- [Notification System](./docs/NOTIFICATIONS.md)

3. **Start the services**

   Each service can be run independently:

   ```bash
   cd src/Backend/DecVCPlat.UserManagement
   dotnet run
   ```

   Or use the provided Docker Compose configuration (if available):

   ```bash
   docker-compose up
   ```

4. **Access the application**

   - Frontend: http://localhost:3000
   - API Gateway: http://localhost:5000
   - Individual services: See service-specific documentation

## Project Structure

```
DecVCPlat/
├── .github/                    # GitHub workflows and templates
├── docs/                       # Documentation files
├── src/                        # Source code
│   ├── Backend/                # Backend services
│   │   ├── DecVCPlat.UserManagement/
│   │   ├── DecVCPlat.ProjectManagement/
│   │   ├── DecVCPlat.Voting/
│   │   ├── DecVCPlat.Funding/
│   │   └── DecVCPlat.Notification/
│   ├── Frontend/               # React frontend application
│   ├── Shared/                 # Shared libraries and utilities
│   ├── DecVCPlat.Accurate.sln  # Solution file
│   └── Setup-Final.ps1         # Setup script
├── tests/                      # Test projects
│   ├── unit/                   # Unit tests
│   ├── integration/            # Integration tests
│   └── e2e/                    # End-to-end tests
├── .gitignore                  # Git ignore file
└── README.md                   # This file
```

## Development Workflow

1. **Branch Strategy**
   - `main`: Production-ready code
   - `develop`: Integration branch for features
   - `feature/*`: Feature branches
   - `bugfix/*`: Bug fix branches
   - `release/*`: Release preparation branches

2. **Commit Guidelines**
   - Use conventional commit messages
   - Reference issue numbers in commits

3. **Pull Request Process**
   - Create PR against `develop` branch
   - Ensure all tests pass
   - Request code review
   - Squash and merge when approved

## Testing

- **Unit Tests**: Test individual components in isolation
  ```bash
  dotnet test tests/unit
  ```

- **Integration Tests**: Test service interactions
  ```bash
  dotnet test tests/integration
  ```

- **End-to-End Tests**: Test complete user workflows
  ```bash
  cd tests/e2e
  npm test
  ```

## Deployment

The platform is configured for CI/CD using GitHub Actions:

1. **Development Environment**: Automatically deployed on merge to `develop`
2. **Staging Environment**: Deployed on release branch creation
3. **Production Environment**: Deployed on merge to `main`

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

Project Link: [https://github.com/vishalishere/DEX-VC-PLAT](https://github.com/vishalishere/DEX-VC-PLAT)

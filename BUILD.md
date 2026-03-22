# Build configuration for Pet App Backend

## Project Structure

```
pet-app-backend/
├── src/
│   ├── api-gateway/
│   │   └── Gateway.Api/
│   ├── building-blocks/
│   │   ├── contracts/
│   │   ├── event-bus/
│   │   └── shared-kernel/
│   └── services/
│       ├── identity-service/
│       ├── user-service/
│       ├── pet-service/
│       ├── file-service/
│       └── notification-service/
├── tests/
│   └── IntegrationTests/
├── docker-compose.yml
├── Dockerfile (per service)
└── Makefile

```

## Build Configurations

### Development
- Target Framework: .NET 10.0
- Configuration: Debug
- Database: PostgreSQL
- Message Bus: RabbitMQ (local)

### Staging
- Target Framework: .NET 10.0
- Configuration: Release
- Database: Azure SQL Database
- Message Bus: Azure Service Bus

### Production
- Target Framework: .NET 10.0
- Configuration: Release
- Database: Azure SQL Database
- Message Bus: Azure Service Bus
- Container Registry: Azure Container Registry

## Build Commands

### Local Development
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Build specific project
dotnet build src/services/identity-service/src/IdentityService.Api/IdentityService.Api.csproj

# Build for release
dotnet build --configuration Release
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=IdentityServiceIntegrationTests"

# Run tests with verbose output
dotnet test --verbosity detailed

# Run tests with code coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Docker Build
```bash
# Build all services
docker-compose build

# Build specific service
docker-compose build identity-service

# Build and push to registry
docker build -t myregistry.azurecr.io/identity-service:latest .
docker push myregistry.azurecr.io/identity-service:latest
```

### Entity Framework
```bash
# Add migration
dotnet ef migrations add MigrationName --project src/services/identity-service/src/IdentityService.Infrastructure

# Update database
dotnet ef database update --project src/services/identity-service/src/IdentityService.Infrastructure

# Remove last migration
dotnet ef migrations remove --project src/services/identity-service/src/IdentityService.Infrastructure
```

## CI/CD Pipeline

### GitHub Actions Workflow (.github/workflows/build-test.yml)

1. **Trigger**: Push to main/develop, Pull requests
2. **Jobs**:
   - Build: Restore, build, run tests
   - Upload coverage reports to Codecov

### Pre-commit Checks
- Code formatting (dotnet format)
- Build verification
- Unit tests pass

### Build Artifacts
- NuGet packages
- Docker images
- Test reports
- Coverage reports

## Configuration Management

### appsettings Files
- `appsettings.json` - Default settings
- `appsettings.Development.json` - Local development
- `appsettings.Staging.json` - Staging environment
- `appsettings.Production.json` - Production environment

### Environment Variables
- ASPNETCORE_ENVIRONMENT: Development|Staging|Production
- ConnectionStrings__ServiceDb
- JwtSettings__SecretKey
- AZURE_SUBSCRIPTION_ID (for Azure resources)

### Secrets Management
- Use Azure Key Vault for production secrets
- Use dotnet user secrets for local development
- Github Secrets for CI/CD pipelines

## Build Targets

| Target | Purpose |
|--------|---------|
| net10.0 | .NET 10.0 support |
| Debug | Development builds with symbols |
| Release | Production builds, optimized |

## Performance Tips

1. **Parallel Build**: Use `dotnet build -m` for faster builds
2. **Incremental Build**: Only rebuild changed projects
3. **NuGet Caching**: Cache ~/.nuget/packages in CI/CD
4. **Docker Layer Caching**: Use multi-stage builds

## Troubleshooting

### Build Fails
- Clear nuget cache: `dotnet nuget locals all --clear`
- Clean project: `dotnet clean`
- Check SDK version: `dotnet --version`

### Test Failures
- Check database connection string
- Verify PostgreSQL is running
- Check RabbitMQ is accessible
- Review test logs for details

### Docker Issues
- Prune unused images: `docker system prune`
- Check port availability
- Verify docker-compose syntax: `docker-compose config`

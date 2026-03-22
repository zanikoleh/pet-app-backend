# Pet App Backend - Deployment Guide

## Table of Contents
1. [Local Development](#local-development)
2. [Docker Deployment](#docker-deployment)
3. [Azure Deployment](#azure-deployment)
4. [Kubernetes Deployment](#kubernetes-deployment)
5. [Environment Configuration](#environment-configuration)
6. [Monitoring and Logging](#monitoring-and-logging)

## Local Development

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL
- Docker Desktop (optional, for local containers)
- Visual Studio 2022 or VS Code

### Setup

1. **Clone and restore**
```bash
git clone <repository-url>
cd pet-app-backend
dotnet restore
```

2. **Initialize databases**
```bash
# Create databases
dotnet ef database update --project src/services/identity-service/src/IdentityService.Infrastructure
dotnet ef database update --project src/services/user-service/src/UserProfileService.Infrastructure
dotnet ef database update --project src/services/file-service/src/FileService.Infrastructure
dotnet ef database update --project src/services/pet-service/src/PetService.Infrastructure
```

3. **Start services**
```bash
# Option 1: Run with docker-compose
docker-compose up

# Option 2: Run individually in separate terminals
cd src/services/identity-service/src/IdentityService.Api && dotnet run
cd src/services/user-service/src/UserProfileService.Api && dotnet run
cd src/services/file-service/src/FileService.Api && dotnet run
cd src/services/pet-service/src/PetService.Api && dotnet run
cd src/services/notification-service/src/NotificationService.Api && dotnet run
cd src/api-gateway/Gateway.Api && dotnet run
```

## Docker Deployment

### Building Docker Images

```bash
# Build all services
docker-compose build

# Build specific service
docker-compose build identity-service

# Build with tag for registry
docker build -t pet-app-backend:identity-service .
```

### Running with Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Remove volumes
docker-compose down -v

# Scale services
docker-compose up -d --scale pet-service=3
```

### Docker networking

Services communicate via internal network `pet-app-network`:
- Identity Service: https://identity-service:443
- User Profile Service: https://user-profile-service:443
- Pet Service: http://pet-service:80
- File Service: https://file-service:443
- Notification Service: https://notification-service:443

## Azure Deployment

### Prerequisites
- Azure subscription
- Azure CLI installed
- Docker configured with Azure Container Registry

### Setup

1. **Create Azure resources**
```bash
# Create resource group
az group create --name pet-app-rg --location eastus

# Create Azure Container Registry
az acr create --resource-group pet-app-rg --name petappregistry --sku Basic

# Create Azure PostgreSQL Database
az postgres server create --name pet-app-db-server --resource-group pet-app-rg --admin-user postgres --admin-password <strong-password>
az postgres db create --name pet-app-identity --server pet-app-db-server --resource-group pet-app-rg
az postgres db create --name pet-app-profile --server pet-app-db-server --resource-group pet-app-rg
az postgres db create --name pet-app-files --server pet-app-db-server --resource-group pet-app-rg
az sql db create --name pet-app-pets --server pet-app-db-server --resource-group pet-app-rg

# Create Azure Service Bus
az servicebus namespace create --name pet-app-bus --resource-group pet-app-rg --sku Standard
az servicebus topic create --namespace-name pet-app-bus --name integration-events --resource-group pet-app-rg

# Create App Service Plan
az appservice plan create --name pet-app-plan --resource-group pet-app-rg --sku B1 --is-linux

# Create App Services
az webapp create --resource-group pet-app-rg --plan pet-app-plan --name pet-app-identity --runtime "DOTNET|10.0"
az webapp create --resource-group pet-app-rg --plan pet-app-plan --name pet-app-profiles --runtime "DOTNET|10.0"
az webapp create --resource-group pet-app-rg --plan pet-app-plan --name pet-app-files --runtime "DOTNET|10.0"
az webapp create --resource-group pet-app-rg --plan pet-app-plan --name pet-app-pets --runtime "DOTNET|10.0"
az webapp create --resource-group pet-app-rg --plan pet-app-plan --name pet-app-gateway --runtime "DOTNET|10.0"
```

2. **Build and push Docker images**
```bash
# Login to Azure Container Registry
az acr login --name petappregistry

# Build and tag images
docker build -f src/services/identity-service/src/IdentityService.Api/Dockerfile -t petappregistry.azurecr.io/identity-service:latest .
docker build -f src/services/user-service/src/UserProfileService.Api/Dockerfile -t petappregistry.azurecr.io/user-profile-service:latest .
docker build -f src/services/file-service/src/FileService.Api/Dockerfile -t petappregistry.azurecr.io/file-service:latest .
docker build -f src/services/pet-service/src/PetService.Api/Dockerfile -t petappregistry.azurecr.io/pet-service:latest .
docker build -f src/services/notification-service/src/NotificationService.Api/Dockerfile -t petappregistry.azurecr.io/notification-service:latest .
docker build -f src/api-gateway/Gateway.Api/Dockerfile -t petappregistry.azurecr.io/api-gateway:latest .

# Push to registry
docker push petappregistry.azurecr.io/identity-service:latest
docker push petappregistry.azurecr.io/user-profile-service:latest
docker push petappregistry.azurecr.io/file-service:latest
docker push petappregistry.azurecr.io/pet-service:latest
docker push petappregistry.azurecr.io/notification-service:latest
docker push petappregistry.azurecr.io/api-gateway:latest
```

3. **Deploy to App Services**
```bash
# Configure web apps to use container
az webapp config container set --name pet-app-identity --resource-group pet-app-rg --docker-custom-image-name petappregistry.azurecr.io/identity-service:latest --docker-registry-server-url https://petappregistry.azurecr.io --docker-registry-server-user <username> --docker-registry-server-password <password>

# Repeat for other services...

# Restart web apps
az webapp restart --name pet-app-identity --resource-group pet-app-rg
# Repeat for other services...
```

4. **Configure application settings**
```bash
# Set connection strings and environment variables
az webapp config appsettings set --name pet-app-identity --resource-group pet-app-rg --settings \
  ConnectionStrings__IdentityServiceDb="<azure-sql-connection-string>" \
  JwtSettings__SecretKey="<jwt-secret-key>" \
  AzureServiceBus__ConnectionString="<service-bus-connection-string>"

# Repeat for other services...
```

## Kubernetes Deployment

### Prerequisites
- Azure Kubernetes Service (AKS) cluster
- kubectl installed
- Images pushed to container registry

### Deploy

1. **Create Kubernetes manifests**
See `k8s/` directory for sample manifests.

2. **Deploy to cluster**
```bash
# Create namespace
kubectl create namespace pet-app

# Deploy services
kubectl apply -f k8s/identity-service.yaml -n pet-app
kubectl apply -f k8s/user-profile-service.yaml -n pet-app
kubectl apply -f k8s/file-service.yaml -n pet-app
kubectl apply -f k8s/pet-service.yaml -n pet-app
kubectl apply -f k8s/notification-service.yaml -n pet-app
kubectl apply -f k8s/api-gateway.yaml -n pet-app

# Verify deployments
kubectl get deployments -n pet-app
kubectl get services -n pet-app

# View logs
kubectl logs -f deployment/identity-service -n pet-app
```

3. **Configure ingress**
```bash
kubectl apply -f k8s/ingress.yaml -n pet-app
```

## Environment Configuration

### Development
```json
{
  "ASPNETCORE_ENVIRONMENT": "Development",
  "ConnectionStrings": {
    "ServiceDb": "Host=localhost;Database=...;Username=postgres;Password=...;Port=5432;"
  },
  "JwtSettings": {
    "SecretKey": "dev-key",
    "Issuer": "pet-app-dev",
    "Audience": "pet-app-api-dev"
  }
}
```

### Staging
```json
{
  "ASPNETCORE_ENVIRONMENT": "Staging",
  "ConnectionStrings": {
    "ServiceDb": "Host=pet-app-db.postgres.database.azure.com;Database=...;Username=postgres@pet-app-db;Port=5432;..."
  },
  "JwtSettings": {
    "SecretKey": "staging-key-from-keyvault",
    "Issuer": "pet-app-staging",
    "Audience": "pet-app-api-staging"
  }
}
```

### Production
```json
{
  "ASPNETCORE_ENVIRONMENT": "Production",
  "ConnectionStrings": {
    "ServiceDb": "Host=pet-app-db.postgres.database.azure.com;Database=...;Username=postgres@pet-app-db;Port=5432;..."
  },
  "JwtSettings": {
    "SecretKey": "production-key-from-keyvault",
    "Issuer": "pet-app-production",
    "Audience": "pet-app-api"
  },
  "AzureServiceBus": {
    "ConnectionString": "Endpoint=sb://pet-app-bus.servicebus.windows.net/..."
  }
}
```

## Monitoring and Logging

### Application Insights
```csharp
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
```

### Structured Logging
```csharp
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.ApplicationInsights(telemetryClient, TelemetryConverter.Traces));
```

### Health Checks
```bash
# Check service health
curl https://localhost:44301/health
curl https://localhost:44302/health
curl https://localhost:44303/health
curl https://localhost:44304/health
curl https://localhost:5000/health
```

### Performance Monitoring
- Monitor database query performance
- Track API response times
- Monitor message bus throughput
- Set up alerts for errors and exceptions

## Scaling Strategies

### Horizontal Scaling
- Use multiple instances of stateless services
- Load balancer distributes requests
- Deploy to Kubernetes for auto-scaling

### Caching
- Implement Redis for response caching
- Cache user profiles
- Cache file metadata

### Database Optimization
- Add indices for frequently queried fields
- Archive old data
- Use read replicas for reports

## Security Checklist

- [x] Use HTTPS for all communication
- [x] Implement JWT token authentication
- [x] Validate all inputs
- [x] Use parameterized queries (EF Core)
- [x] Implement rate limiting
- [x] Use environment variables for secrets
- [x] Store secrets in Key Vault
- [ ] Implement CORS properly (configure allowed origins)
- [ ] Add API versioning
- [ ] Implement audit logging
- [ ] Regular security patches
- [ ] Regular backups

## Rollback Procedure

```bash
# If deployment fails, rollback to previous version
docker-compose down
git checkout previous-version
docker-compose build
docker-compose up
```

## Support

For deployment issues, check:
1. Application logs: Review application insights or docker logs
2. Service health endpoints: Verify all services are responding
3. Database connectivity: Test database connection
4. Message bus: Verify service bus is accessible
5. DNS resolution: Check service discovery

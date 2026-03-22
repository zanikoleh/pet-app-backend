# Pet App Backend - Microservices Architecture

## Overview

This is a comprehensive microservices backend for the Pet App, built with .NET 10 using Domain-Driven Design (DDD) and Clean Architecture principles.

## Architecture

### Services

1. **Identity Service** (Port 44301)
   - User registration and authentication
   - JWT token management
   - OAuth provider integration (Google, Facebook, Apple)
   - Refresh token rotation

2. **User Profile Service** (Port 44302)
   - User profile management
   - Notification preferences
   - Language and timezone settings

3. **Pet Service** (Port 5000)
   - Pet records management
   - Pet photos and documents
   - Vaccination and medical records

4. **File Service** (Port 44303)
   - File upload and storage
   - Virus scanning
   - Signed download URLs
   - Azure Blob Storage support

5. **Notification Service** (Port 44304)
   - Email notifications
   - SMS notifications
   - Event-driven notifications from other services

6. **API Gateway** (Port 44300)
   - Single entry point for all services
   - Request routing
   - Cross-cutting concerns

### Supporting Infrastructure

- **PostgreSQL**: Multi-database per service pattern
- **Azure Service Bus**: Event-driven communication between services
- **RabbitMQ**: Local development message broker alternative

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker and Docker Compose
- PostgreSQL
- Visual Studio 2022 or VS Code

### Local Development

#### Using Docker Compose (Recommended)

```bash
# Start all services with dependencies
docker-compose up --build

# Services will be available at:
# - API Gateway: https://localhost:44300
# - Identity Service: https://localhost:44301
# - User Profile Service: https://localhost:44302
# - File Service: https://localhost:44303
# - Notification Service: https://localhost:44304
# - Pet Service: http://localhost:5000
```

#### Manual Setup

```bash
# 1. Install dependencies
dotnet restore

# 2. Build the solution
dotnet build

# 3. Setup databases
cd src/services/identity-service/src/IdentityService.Infrastructure
dotnet ef database update

# 4. Run individual services
cd src/services/identity-service/src/IdentityService.Api
dotnet run
```

## API Documentation

Each service exposes Swagger/OpenAPI documentation:

- Identity Service: https://localhost:44301/swagger
- User Profile Service: https://localhost:44302/swagger
- Pet Service: http://localhost:5000/swagger
- File Service: https://localhost:44303/swagger

## Authentication

Most endpoints require JWT authentication. Get a token from the Identity Service:

```bash
curl -X POST https://localhost:44301/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "fullName": "John Doe"
  }'
```

Then use the token in subsequent requests:

```bash
curl -X GET https://localhost:44301/api/auth/profile \
  -H "Authorization: Bearer <access_token>"
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/IntegrationTests/IntegrationTests.csproj

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Configuration

### appsettings.json

Key configurations:

```json
{
  "ConnectionStrings": {
    "ServiceDb": "Host=localhost;Database=...;Username=postgres;Password=...;Port=5432;"
  },
  "JwtSettings": {
    "SecretKey": "your-256-bit-key",
    "Issuer": "pet-app-identity-service",
    "Audience": "pet-app-api",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationMinutes": 10080
  },
  "AzureServiceBus": {
    "ConnectionString": "Endpoint=..."
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Development, Staging, Production
- `ConnectionStrings__ServiceDb`: Database connection string
- `JwtSettings__SecretKey`: JWT signing key

## Database Schema

### Users (Identity Service)
- User aggregate with email/password and OAuth providers
- Refresh tokens with revocation and expiration
- OAuth provider links

### UserProfiles (User Profile Service)
- User profile with personal information
- User preferences (notifications, language, timezone)

### Pets (Pet Service)
- Pet records with basic information
- Photos, medical records, vaccinations

### Files (File Service)
- File records with storage paths
- Virus scan status
- User and entity associations

### Notifications (Notification Service)
- Email and SMS logs (if persisted)
- Delivery status

## Development Workflow

1. **Feature Development**
   - Create feature branch: `git checkout -b feature/feature-name`
   - Make changes following DDD principles
   - Add/update domain events
   - Create application layer commands/queries
   - Implement repository pattern
   - Add API endpoints

2. **Testing**
   - Unit tests for domain logic
   - Integration tests for application services
   - Controller tests for API endpoints

3. **Code Style**
   - Follow C# naming conventions
   - Use nullable reference types
   - Keep methods small and focused
   - Document public APIs with XML comments

## Troubleshooting

### Services won't start
- Check Docker is running: `docker ps`
- Check logs: `docker-compose logs identity-service`
- Verify ports are available: `netstat -an | grep LISTEN`

### Database connection issues
- Verify PostgreSQL is running
- Check connection string in appsettings
- Check firewall allows port 5432

### Event bus not working
- Check RabbitMQ is running: `docker ps | grep rabbitmq`
- Check RabbitMQ UI: http://localhost:15672

## Future Enhancements

- [ ] Add caching layer (Redis)
- [ ] Implement service discovery (Consul)
- [ ] Add distributed tracing (Jaeger)
- [ ] Implement rate limiting
- [ ] Add API versioning
- [ ] Enhance error handling and logging
- [ ] Add performance monitoring
- [ ] Implement CQRS event sourcing
- [ ] Add GraphQL endpoint
- [ ] Kubernetes deployment configs

## License

Proprietary - Pet App

## Support

For issues and questions, contact the development team.
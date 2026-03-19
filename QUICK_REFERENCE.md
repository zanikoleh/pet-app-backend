# Pet App Backend - Developer Quick Reference

## 🚀 Quick Start

### First Time Setup
```bash
# Clone repository
git clone <repo-url>
cd pet-app-backend

# Restore and build
dotnet restore
dotnet build

# Start services
docker-compose up

# Services ready at:
# - Gateway: https://localhost:44300
# - Identity: https://localhost:44301
# - Profile: https://localhost:44302
# - Files: https://localhost:44303
# - Pets: http://localhost:5000
# - Notifications: https://localhost:44304
```

## 📋 Common Tasks

### Running Services

```bash
# All services
docker-compose up -d

# Specific service
docker-compose up -d identity-service

# View logs
docker-compose logs -f identity-service

# Stop all
docker-compose down

# Health check
./health-check.sh
```

### Building & Testing

```bash
# Build all
dotnet build

# Build specific project
dotnet build src/services/identity-service/src/IdentityService.Api

# Run tests
dotnet test

# Run specific test
dotnet test --filter "ClassName=IdentityServiceIntegrationTests"

# With coverage
dotnet test /p:CollectCoverage=true
```

### Database Management

```bash
# Add migration
dotnet ef migrations add InitialCreate \
  --project src/services/identity-service/src/IdentityService.Infrastructure \
  --startup-project src/services/identity-service/src/IdentityService.Api

# Update database
dotnet ef database update \
  --project src/services/identity-service/src/IdentityService.Infrastructure \
  --startup-project src/services/identity-service/src/IdentityService.Api

# Remove last migration
dotnet ef migrations remove \
  --project src/services/identity-service/src/IdentityService.Infrastructure
```

### Adding a New Feature

1. **Update Domain Layer** (`Domain/`)
   - Add entity or aggregate
   - Add value objects
   - Add domain events

2. **Update Application Layer** (`Application/`)
   - Add command/query in Commands.cs or Queries.cs
   - Add handler in Handlers/
   - Add DTO in DTOs/
   - Add validator in Validators/

3. **Update Infrastructure Layer** (`Infrastructure/`)
   - Add repository method if needed
   - Add event subscriber if publishing events

4. **Add API Endpoint** (`Api/Controllers/`)
   - Add controller action
   - Add route
   - Add authorization if needed

5. **Test**
   - Add integration test
   - Add domain tests
   - Run full test suite

## 📁 Project Structure Reference

```
src/
├── api-gateway/
│   └── Gateway.Api/
│       ├── Program.cs              # Ocelot setup
│       ├── ocelot.json             # Routes
│       └── appsettings.json        # Configuration
│
├── building-blocks/
│   ├── contracts/                  # Shared DTOs & events
│   │   └── events/
│   ├── event-bus/                  # Event publishing
│   └── shared-kernel/              # DDD base classes
│
└── services/
    ├── identity-service/
    │   └── src/
    │       ├── IdentityService.Api/
    │       │   ├── Program.cs
    │       │   ├── Controllers/AuthController.cs
    │       │   └── appsettings*.json
    │       ├── IdentityService.Application/
    │       │   ├── Commands/AuthenticationCommands.cs
    │       │   ├── Queries/AuthenticationQueries.cs
    │       │   ├── Handlers/
    │       │   ├── DTOs/
    │       │   └── Validators/
    │       ├── IdentityService.Domain/
    │       │   ├── Entities/User.cs
    │       │   ├── Events/
    │       │   └── ValueObjects/
    │       └── IdentityService.Infrastructure/
    │           ├── Persistence/
    │           ├── Services/
    │           └── DependencyInjection.cs
    │
    ├── user-service/                # UserProfile Service
    ├── pet-service/                 # Pet Service
    ├── file-service/                # File Service
    └── notification-service/        # Notification Service

tests/
└── IntegrationTests/
    ├── Services/                    # Service integration tests
    ├── Domain/                      # Domain model tests
    └── IntegrationTestsFixture.cs
```

## 🎨 Code Patterns

### Creating a Command Handler

```csharp
public class UpdateUserProfileCommandHandler : 
    IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    private readonly IUserProfileRepository _repository;
    private readonly IMapper _mapper;

    public UpdateUserProfileCommandHandler(
        IUserProfileRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        // Validate
        var userProfile = await _repository.GetByUserIdAsync(request.UserId);
        if (userProfile is null)
            return Result<UserProfileDto>.Failure("Profile not found");

        // Update
        userProfile.Update(request.FirstName, request.LastName, request.Bio);

        // Save
        await _repository.UpdateAsync(userProfile);

        // Map & Return
        var dto = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(dto);
    }
}
```

### Creating a Query Handler

```csharp
public class GetUserProfileQueryHandler : 
    IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IUserProfileRepository _repository;
    private readonly IMapper _mapper;

    public async Task<Result<UserProfileDto>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userProfile = await _repository.GetByUserIdAsync(request.UserId);
        if (userProfile is null)
            return Result<UserProfileDto>.Failure("Not found");

        var dto = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(dto);
    }
}
```

### Creating a Domain Event Handler

```csharp
public class UserRegisteredEventHandler : 
    INotificationHandler<UserRegisteredIntegrationEvent>
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public async Task Handle(
        UserRegisteredIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating profile for new user {UserId}", 
            notification.UserId);

        await _userProfileService.CreateProfileAsync(
            notification.UserId,
            notification.Email,
            cancellationToken);
    }
}
```

### Creating an API Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile(int userId)
    {
        var query = new GetUserProfileQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateProfile(
        int userId,
        UpdateUserProfileRequest request)
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
```

### Creating a Validator

```csharp
public class UpdateUserProfileCommandValidator : 
    AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

## 🔑 Key Files Location

| Task | File |
|------|------|
| Add route to API Gateway | `src/api-gateway/Gateway.Api/ocelot.json` |
| Change JWT settings | `src/services/*/appsettings.json` |
| Update database schema | `src/services/*/Infrastructure/Persistence/*Configuration.cs` |
| Add domain event | `src/services/*/Domain/Events/` |
| Register services | `src/services/*/Infrastructure/DependencyInjection.cs` |
| Configure database | `src/services/*/Api/Program.cs` |
| Configure middleware | `src/services/*/Api/Program.cs` |

## 🧪 Testing Checklist

- [ ] Unit tests for domain logic
- [ ] Integration tests for handlers
- [ ] Integration tests for API endpoints
- [ ] Validate happy path flows
- [ ] Validate error scenarios
- [ ] Test with invalid inputs
- [ ] Test database operations
- [ ] Test event publishing
- [ ] All tests pass locally
- [ ] Code coverage acceptable (>70%)

## 🐛 Debugging Tips

### Enable Debug Logging
```csharp
// In Program.cs
builder.Host.ConfigureLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

### Add Debug Breakpoints
1. Set breakpoint in Visual Studio
2. Run service in debug mode
3. Make request to trigger breakpoint

### View Database
```bash
# Connect to SQL Server
sqlcmd -S localhost -U sa -P PetApp123!@#

# List databases
SELECT name FROM sys.databases;

# Use database
USE PetApp.IdentityService;

# View table
SELECT * FROM Users;
```

### View Message Bus
```bash
# RabbitMQ Management UI
http://localhost:15672
# Default credentials: guest / guest

# View queues and topics
# Check message rates and acknowledgments
```

### Check Logs
```bash
# Docker logs
docker-compose logs identity-service

# Live logs
docker-compose logs -f identity-service

# All services
docker-compose logs

# Last 100 lines
docker-compose logs --tail=100
```

## 📚 Important URLs (Local Development)

| Service | URL | Swagger |
|---------|-----|---------|
| API Gateway | https://localhost:44300 | N/A |
| Identity | https://localhost:44301 | https://localhost:44301/swagger |
| Profile | https://localhost:44302 | https://localhost:44302/swagger |
| Files | https://localhost:44303 | https://localhost:44303/swagger |
| Pets | http://localhost:5000 | http://localhost:5000/swagger |
| Notifications | https://localhost:44304 | https://localhost:44304/swagger |
| RabbitMQ | http://localhost:15672 | (Management UI) |
| SQL Server | localhost:1433 | (SSMS) |

## 👤 Default Credentials

| Service | User | Password |
|---------|------|----------|
| SQL Server | sa | PetApp123!@# |
| RabbitMQ | guest | guest |

## 💾 Database Connections

### Local Development
```
Server=localhost
Database=PetApp.[ServiceName]
User=sa
Password=PetApp123!@#
```

### Connection String with SQL Client
```
Server=localhost;Database=PetApp.IdentityService;User Id=sa;Password=PetApp123!@#;Encrypt=False;TrustServerCertificate=True;
```

## 🚨 Common Issues & Solutions

### Issue: "Port already in use"
**Solution**: 
```bash
# Kill process on port
sudo lsof -ti:44301 | xargs kill -9
# Or change port in docker-compose.yml
```

### Issue: "Database connection failed"
**Solution**:
```bash
# Verify SQL Server is running
docker ps | grep sqlserver

# Check connection string
docker-compose logs sqlserver
```

### Issue: "Service not accessible"
**Solution**:
```bash
# Check if service is running
docker ps

# Check logs
docker-compose logs service-name

# Verify network
docker network ls
docker network inspect pet-app-backend_pet-app-network
```

### Issue: "JWT validation failed"
**Solution**:
```bash
# Verify JWT secret key matches across services
# Check in appsettings.json for each service
grep "SecretKey" src/services/*/appsettings.json

# Ensure token expiration hasn't passed
# Refresh token if needed
```

## 🔄 Development Workflow

1. **Create feature branch**
   ```bash
   git checkout -b feature/my-feature
   ```

2. **Make changes**
   - Update domain
   - Update application
   - Update infrastructure
   - Add API endpoint

3. **Test locally**
   ```bash
   docker-compose up
   dotnet test
   ```

4. **Commit with meaningful message**
   ```bash
   git commit -m "feat: add user profile update feature"
   ```

5. **Push and create PR**
   ```bash
   git push origin feature/my-feature
   ```

6. **CI/CD runs automatically**
   - Tests run
   - Build verified
   - Coverage checked

7. **Merge after approval**

## 📞 Getting Help

1. Check `README.md` for general info
2. Check `ARCHITECTURE.md` for design details
3. Review `DEPLOYMENT.md` for operational info
4. Check existing tests for code patterns
5. Look at similar features for reference

---

**Last Updated**: Phase 6 Complete
**Version**: 1.0.0

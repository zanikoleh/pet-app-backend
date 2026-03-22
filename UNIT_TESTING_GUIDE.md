# Unit Testing Implementation Guide - Pet App Backend

## Overview

This document provides comprehensive guidance on the unit test structure created for the Pet App Backend microservices. The framework implements DDD (Domain-Driven Design) best practices using xUnit, FluentAssertions, and Moq.

## Project Structure

All test projects follow a consistent pattern with parallel directory structure:

```
tests/
├── {ServiceName}.Domain.Tests/               # Domain layer tests
│   ├── Aggregates/                           # Aggregate root tests
│   ├── Entities/                             # Entity tests
│   ├── ValueObjects/                         # Value object tests
│   ├── Specifications/                       # Specification pattern tests
│   ├── Builders/                             # Test data builders
│   ├── Fixtures/                             # Shared test setup
│   ├── Mocks/                                # Mock implementations
│   └── Helpers/                              # Test utilities
├── {ServiceName}.Application.Tests/          # Application (CQRS) layer tests
│   ├── Commands/                             # Command handler tests
│   ├── Queries/                              # Query handler tests
│   ├── Handlers/                             # Handler tests (or combined)
│   ├── Builders/                             # DTO and command builders
│   ├── Fixtures/                             # Shared AutoMapper setup
│   ├── Mocks/                                # Mock repositories/services
│   └── Helpers/                              # Test utilities
├── {ServiceName}.Infrastructure.Tests/       # Infrastructure layer tests
│   ├── Repositories/                         # Repository tests
│   ├── Persistence/                          # DbContext tests
│   ├── Services/                             # Custom service tests
│   ├── Fixtures/                             # Database fixtures
│   └── Helpers/                              # Database helpers
└── {ServiceName}.Api.Tests/                  # API layer tests
    ├── Controllers/                          # Controller tests
    ├── Fixtures/                             # WebApplicationFactory setup
    ├── Mocks/                                # MediatR/IMapper mocks
    └── Helpers/                              # HTTP helpers
```

## Testing Layer Breakdown

### 1. Domain Layer Tests (`{Service}.Domain.Tests`)

**Purpose**: Validate core business logic, invariants, and domain events.

**Test Location**: `{ServiceName}.Domain.Tests/Aggregates/`, `Entities/`, `ValueObjects/`

**Example**: [PetService.Domain.Tests/Aggregates/PetAggregateTests.cs](../tests/PetService.Domain.Tests/Aggregates/PetAggregateTests.cs)

**Key Patterns**:

```csharp
// Test aggregate creation and validation
[Fact]
public void Create_ValidInput_ShouldCreateAggregate()
{
    // Arrange
    var input = BuildValidInput();
    
    // Act
    var aggregate = new Aggregate(input);
    
    // Assert
    aggregate.Property.Should().Be(expected);
}

// Test business rules
[Fact]
public void Operation_ViolatesBusinessRule_ShouldThrowDomainException()
{
    // Arrange
    var aggregate = CreateTestAggregate();
    
    // Act & Assert
    var exception = Record.Exception(() => aggregate.PerformInvalidOperation());
    exception.Should().BeOfType<DomainException>();
}

// Test domain event publishing
[Fact]
public void Create_ShouldRaiseDomainEvent()
{
    // Arrange & Act
    var aggregate = new Aggregate(validInput);
    
    // Assert
    var domainEvents = aggregate.DomainEvents.ToList();
    domainEvents.Should().HaveCount(1);
    domainEvents[0].Should().BeOfType<DomainEvent>();
}

// Test value object equality (by value, not reference)
[Fact]
public void Equality_SameValue_ShouldBeEqual()
{
    var vo1 = ValueObject.Create("value");
    var vo2 = ValueObject.Create("value");
    vo1.Should().Be(vo2);
}
```

**Coverage Target**: ≥95%
- All constructor validations
- All public methods
- Boundary conditions
- Domain event publishing
- Aggregate invariants

---

### 2. Application Layer Tests (`{Service}.Application.Tests`)

**Purpose**: Validate CQRS handlers, DTOs, mappings, and business orchestration.

**Test Location**: `{ServiceName}.Application.Tests/Handlers/`, `Commands/`, `Queries/`

**Example**: [PetService.Application.Tests/Handlers/CommandHandlerTests.cs](../tests/PetService.Application.Tests/Handlers/CommandHandlerTests.cs)

**Key Patterns**:

```csharp
// Test command handler success path
[Fact]
public async Task Handle_ValidCommand_ShouldProcessAndReturnResult()
{
    // Arrange
    var handler = new CreateCommandHandler(
        _repositoryMock.Object,
        _mapperMock.Object);
    
    var command = new CreateCommand(validInput);
    
    var expectedDto = new Dto { /* properties */ };
    
    _repositoryMock
        .Setup(r => r.AddAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    
    _mapperMock
        .Setup(m => m.Map<Dto>(It.IsAny<Entity>()))
        .Returns(expectedDto);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.Property.Should().Be(expectedDto.Property);
    
    _repositoryMock.Verify(
        r => r.AddAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
        Times.Once);
}

// Test handler error handling
[Fact]
public async Task Handle_EntityNotFound_ShouldThrowNotFoundException()
{
    // Arrange
    var handler = new UpdateCommandHandler(_repositoryMock.Object, _mapperMock.Object);
    var command = new UpdateCommand(nonExistentId);
    
    _repositoryMock
        .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Entity)null!);
    
    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => handler.Handle(command, CancellationToken.None));
}

// Test authorization checks
[Fact]
public async Task Handle_DifferentOwner_ShouldThrowUnauthorizedException()
{
    // Arrange
    var handler = new DeleteCommandHandler(_repositoryMock.Object);
    var command = new DeleteCommand(entityId, wrongOwnerId);
    
    var entity = CreateEntity(ownerId: correctOwnerId);
    
    _repositoryMock
        .Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);
    
    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => handler.Handle(command, CancellationToken.None));
}

// Test query handler with pagination
[Fact]
public async Task Handle_GetPaginatedQuery_ShouldReturnPagedResult()
{
    // Arrange
    var handler = new GetPaginatedQueryHandler(_repositoryMock.Object, _mapperMock.Object);
    var query = new GetPaginatedQuery(pageNumber: 1, pageSize: 10);
    
    var entities = CreateTestEntities(count: 15);
    var expectedDtos = MapToDtos(entities.Take(10));
    
    _repositoryMock
        .Setup(r => r.GetPaginatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((expectedDtos, totalCount: 15));
    
    // Act
    var result = await handler.Handle(query, CancellationToken.None);
    
    // Assert
    result.Items.Should().HaveCount(10);
    result.TotalCount.Should().Be(15);
}
```

**Mocking Strategy**:

- **IRepository**: Mock all CRUD methods (GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, SaveChangesAsync)
- **IMapper**: Mock Map<TDto>() for DTO transformations
- **IEventPublisher**: Mock for event publishing verification
- **IMediator**: Mock for nested command/query handling (if applicable)

**Coverage Target**: ≥85%
- Handler success paths
- Validation failures
- Authorization checks
- Error conditions
- DTO mappings

---

### 3. Infrastructure Layer Tests (`{Service}.Infrastructure.Tests`)

**Purpose**: Validate repositories, database persistence, and data access patterns.

**Test Location**: `{ServiceName}.Infrastructure.Tests/Repositories/`, `Persistence/`

**Key Patterns**:

```csharp
// Test repository CRUD operations with LocalDB
[Fact]
public async Task AddAsync_ValidEntity_ShouldPersistToDatabase()
{
    // Arrange
    using var dbContext = CreateTestDbContext();
    var repository = new EntityRepository(dbContext);
    var entity = CreateTestEntity();
    
    // Act
    await repository.AddAsync(entity, CancellationToken.None);
    await repository.SaveChangesAsync(CancellationToken.None);
    
    // Assert
    using var verifyContext = CreateTestDbContext(_dbName);
    var persisted = await verifyContext.Entities.FirstOrDefaultAsync(e => e.Id == entity.Id);
    persisted.Should().NotBeNull();
    persisted.Property.Should().Be(entity.Property);
}

// Test repository queries with specifications
[Fact]
public async Task GetAsync_WithSpecification_ShouldReturnFilteredResults()
{
    // Arrange
    using var dbContext = CreateTestDbContext();
    var repository = new EntityRepository(dbContext);
    
    // Seed test data
    var entity1 = CreateEntityWithStatus("Active");
    var entity2 = CreateEntityWithStatus("Inactive");
    await repository.AddAsync(entity1, CancellationToken.None);
    await repository.AddAsync(entity2, CancellationToken.None);
    await repository.SaveChangesAsync(CancellationToken.None);
    
    var spec = new ActiveEntitiesSpecification();
    
    // Act
    var results = await repository.GetAsync(spec, CancellationToken.None);
    
    // Assert
    results.Should().HaveCount(1);
    results.First().Status.Should().Be("Active");
}

// Test DbContext configuration
[Fact]
public void DbContext_Configuration_ShouldHaveCorrectMappings()
{
    // Arrange
    using var dbContext = CreateTestDbContext();
    
    // Act
    var model = dbContext.Model;
    var entityType = model.FindEntityType(typeof(MyEntity));
    
    // Assert
    entityType.Should().NotBeNull();
    entityType.GetProperties().Should().Contain(p => p.Name == "Property");
}
```

**Database Setup**:

```csharp
// Use LocalDB for infrastructure tests
private static string CreateTestDatabaseName() => 
    $"TestDb_{Guid.NewGuid():N}";

protected DbContext CreateTestDbContext(string? databaseName = null)
{
    var connectionString = 
        $"Server=(localdb)\\mssqllocaldb;Database={databaseName ?? CreateTestDatabaseName()};Integrated Security=true;";
    
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .Options;
    
    var context = new AppDbContext(options);
    context.Database.EnsureCreated();
    
    return context;
}
```

**Coverage Target**: ≥75%
- CRUD operations
- Complex queries with specifications
- Transaction handling
- Constraint validation
- Relationship handling

---

### 4. API Layer Tests (`{Service}.Api.Tests`)

**Purpose**: Validate HTTP endpoints, request/response mapping, and error handling.

**Test Location**: `{ServiceName}.Api.Tests/Controllers/`

**Key Patterns**:

```csharp
// Test controller action success
[Fact]
public async Task Get_ValidId_ReturnsOkWithDto()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    using var client = factory.CreateClient();
    
    var entityId = Guid.NewGuid();
    var expectedDto = new EntityDto { Id = entityId };
    
    var mockMediator = factory.Services.GetRequiredService<Mock<IMediator>>();
    mockMediator
        .Setup(m => m.Send(It.IsAny<GetQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedDto);
    
    // Act
    var response = await client.GetAsync($"/api/entities/{entityId}");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsAsync<EntityDto>();
    content.Id.Should().Be(entityId);
}

// Test validation errors
[Fact]
public async Task Create_InvalidRequest_ReturnsBadRequest()
{
    // Arrange
    using var client = new WebApplicationFactory<Program>().CreateClient();
    var invalidRequest = new CreateRequest { Name = "" }; // Missing required field
    
    // Act
    var response = await client.PostAsJsonAsync("/api/entities", invalidRequest);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

// Test authorization
[Fact]
public async Task Delete_WithoutAuth_ReturnsUnauthorized()
{
    // Arrange
    using var client = new WebApplicationFactory<Program>().CreateClient();
    // Don't add auth header
    
    // Act
    var response = await client.DeleteAsync($"/api/entities/{Guid.NewGuid()}");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}

// Test not found
[Fact]
public async Task Update_NonExistentId_ReturnsNotFound()
{
    // Arrange
    using var client = new WebApplicationFactory<Program>().CreateClient();
    var request = new UpdateRequest { Name = "Updated" };
    var nonExistentId = Guid.NewGuid();
    
    // Act
    var response = await client.PutAsJsonAsync($"/api/entities/{nonExistentId}", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

**Controller Test Setup**:

```csharp
public class EntityControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public EntityControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real services with mocks if needed
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (descriptor != null)
                    services.Remove(descriptor);
                
                services.AddScoped(sp => new Mock<IMediator>().Object);
            });
        });
    }
}
```

**Coverage Target**: ≥80%
- Success responses (200, 201, 204)
- Client errors (400, 401, 403, 404)
- Server errors (500)
- Request validation
- Response mapping
- Authorization checks

---

## Test Data Builders

Builders provide fluent interfaces for creating test objects with sensible defaults:

```csharp
// Domain entity builder
var pet = new PetAggregateBuilder()
    .WithName("Fluffy")
    .WithType("Cat")
    .WithBreed("Siamese")
    .Build();

// Command builder
var command = new CreateCommandBuilder()
    .WithOwnerId(testOwnerId)
    .WithName("MaxFull")
    .Build();

// DTO builder
var dto = new DTOBuilder()
    .WithId(testId)
    .WithName("TestName")
    .Build();
```

**Location**: `{Service}.{Layer}.Tests/Builders/`

---

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run specific service tests
```bash
dotnet test tests/PetService.Domain.Tests/
dotnet test tests/PetService.Application.Tests/
```

### Run with coverage
```bash
# Requires coverlet
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run and filter
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "FullyQualifiedName~PetAggregateTests"
```

---

## Extending to Other Services

For each of the 5 services (Identity, User, File, Notification), follow this sequence:

### 1. Domain Tests

1. Read the existing domain layer code
2. Create tests in `{Service}.Domain.Tests/Aggregates/`
3. Identify invariants, value objects, and domain events
4. Create test cases for:
   - Construction and validation
   - Business rule enforcement
   - Domain event publishing
   - Entity relationships

### 2. Application Tests

1. Identify all command and query handlers
2. Create tests in `{Service}.Application.Tests/Handlers/`
3. Mock repositories and dependencies
4. Test:
   - Success paths
   - Validation failures
   - Authorization
   - Error handling

### 3. Infrastructure Tests

1. Create DbContext test fixture
2. Create tests in `{Service}.Infrastructure.Tests/Repositories/`
3. Test with LocalDB
4. Verify:
   - Persistence
   - Query specifications
   - Relationships

### 4. API Tests

1. Create WebApplicationFactory setup
2. Create tests in `{Service}.Api.Tests/Controllers/`
3. Test all HTTP endpoints
4. Verify status codes and responses

---

## Best Practices

### ✅ DO

- Write **one assertion per test** (except for integration/comprehensive tests)
- Use **descriptive test names**: `Method_Scenario_ExpectedResult`
- **Mock external dependencies** (databases, APIs, event bus)
- **Setup defaults** in builders and fixtures
- **Test edge cases** and boundary conditions
- **Verify method calls** on mocks to ensure orchestration
- **Use CancellationToken.None** in tests unless testing cancellation

### ❌ DON'T

- **Don't test trivial getters/setters**
- **Don't use actual databases** in unit tests (except infrastructure layer)
- **Don't test multiple concerns** in one test
- **Don't ignore test failures**
- **Don't have tests call each other**
- **Don't use magic numbers** without explanation
- **Don't test implementation details** instead of behavior

---

## Async/Await Patterns

All handlers are async. Use proper async test patterns:

```csharp
// ✅ Correct
[Fact]
public async Task Handle_ValidInput_ShouldReturnResult()
{
    // ... test code ...
    var result = await handler.Handle(command, CancellationToken.None);
}

// ❌ Wrong - deadlock risk
[Fact]
public void Handle_ValidInput_ShouldReturnResult()
{
    var result = handler.Handle(command, CancellationToken.None).Result; // Potential deadlock
}
```

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| "Cannot convert from 'Task<T>' to 'Task<T>'" in Moq | Setup returns `Task.CompletedTask` not bare method call |
| DateTime comparison failures | Use `.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1))` |
| DomainEvents collection is empty | Remember to call `.DomainEvents.ToList()` to materialize |
| Mock not being called | Check exact parameter matching in `.Setup()` |
| LocalDB connection fails | Verify `(localdb)\mssqllocaldb` instance exists |

---

## Coverage Targets Summary

| Layer | Target | Focus Areas |
|-------|--------|------------|
| **Domain** | ≥95% | Aggregates, value objects, invariants, events |
| **Application** | ≥85% | Handlers, commands, queries, mapping |
| **Infrastructure** | ≥75% | Repositories, DbContext, persistence |
| **API** | ≥80% | Controllers, endpoints, errors, auth |

---

## Next Steps

1. ✅ Complete Pet Service (Domain + Application tests done)
2. Copy Pet Service test structure to other services
3. Adapt tests for service-specific domain models
4. Add Infrastructure tests with LocalDB
5. Add API endpoint tests
6. Achieve target coverage for all layers
7. Configure CI/CD to run tests on every commit

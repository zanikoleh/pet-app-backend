# Pet App Backend Architecture

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                           Client Applications                        │
│                      (Web, Mobile, Desktop)                          │
└────────────────────────┬──────────────────────────────────────────────┘
                         │ HTTPS Request
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     API Gateway (Ocelot)                             │
│                    Port: 44300 (HTTPS)                               │
│   ┌─────────────────────────────────────────────────────────────┐   │
│   │ • Request Routing                                            │   │
│   │ • CORS Configuration                                         │   │
│   │ • Rate Limiting                                              │   │
│   │ • Load Balancing                                             │   │
│   └─────────────────────────────────────────────────────────────┘   │
└────────────┬────────────┬─────────────┬──────────────┬───────────────┘
             │            │             │              │
    ┌────────▼─┐  ┌──────▼──┐  ┌──────▼──┐  ┌────────▼──┐  ┌──────────┐
    │ Identity │  │ Profile │  │  File   │  │   Pets    │  │Notif.    │
    │ Service  │  │ Service │  │ Service │  │  Service  │  │ Service  │
    │44301 HTTPS│ │44302HTTPS│ │44303HTTPS│ │5000 HTTP│  │44304HTTPS│
    └────┬──────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬────┘
         │              │             │             │             │
         ├──────────────┴─────────────┴─────────────┴─────────────┤
         │                                                          │
         ▼──────────────────────────────────────────────────────┐  │
    ┌───────────────────────────────────────────────────────┐  │  │
    │           Azure Service Bus / RabbitMQ                 │  │  │
    │     (Event-Driven Inter-Service Communication)         │  │  │
    │                                                         │  │  │
    │  Integration Events:                                   │  │  │
    │  ┌─ UserRegistered → Profile Created, Email Sent   ──┐│  │  │
    │  ├─ UserDeleted → Profile Deactivated, Email Sent   ─┤│  │  │
    │  ├─ UserProfileUpdated → Notification Sent           ─┤│  │  │
    │  └─ NotificationPreferencesUpdated → Confirmation    ──┘│  │  │
    └───────────────────────────────────────────────────────┘  │  │
         │                                                      │  │
         ▼──────────────────────────────────┬───────────────────┘  │
    ┌──────────────────────────────────┐   │                      │
    │      SQL Server Database         │   │                      │
    │                                  │   │                      │
    │  ┌──────────────────────────┐   │   │                      │
    │  │ Identity-Service DB      │   │   │                      │
    │  │ • Users                  │   │   │                      │
    │  │ • OAuthProviderLinks     │   │   │                      │
    │  │ • RefreshTokens          │   │   │                      │
    │  └──────────────────────────┘   │   │                      │
    │                                  │   │                      │
    │  ┌──────────────────────────┐   │   │                      │
    │  │ UserProfile-Service DB   │   │   │                      │
    │  │ • UserProfiles           │   │   │                      │
    │  │ • UserPreferences        │   │   │                      │
    │  └──────────────────────────┘   │   │                      │
    │                                  │   │                      │
    │  ┌──────────────────────────┐   │   │                      │
    │  │ Pet-Service DB           │   │   │                      │
    │  │ • Pets                   │   │   │                      │
    │  │ • Vaccinations           │   │   │                      │
    │  └──────────────────────────┘   │   │                      │
    │                                  │   │                      │
    │  ┌──────────────────────────┐   │   │                      │
    │  │ File-Service DB          │   │   │                      │
    │  │ • FileRecords            │   │   │                      │
    │  │ • StorageMetadata        │   │   │                      │
    │  └──────────────────────────┘   │   │                      │
    │                                  │   │                      │
    │  ┌──────────────────────────┐   │   │                      │
    │  │ External Services        │   │   │                      │
    │  │ • Azure Blob Storage     │   │   │                      │
    │  │ • Email Service          │◄──┼──┼──────────────────────┘
    │  │ • SMS Service            │   │   │
    │  │ • OAuth Providers        │   │   │
    │  └──────────────────────────┘   │   │
    │                                  │   │
    └──────────────────────────────────┘   │
                                           │
                            (Queries/Commands)
```

## Service Architecture Pattern

Each microservice follows identical layered architecture:

```
┌─────────────────────────────────────────────┐
│           API Layer (Controllers)            │
│  • REST Endpoints                            │
│  • HTTP Request/Response Handling            │
│  • JWT Authentication Middleware             │
└────────────────┬────────────────────────────┘
                 │ Commands/Queries
┌────────────────▼────────────────────────────┐
│      Application Layer (CQRS)                │
│  • Commands (state-changing operations)      │
│  • Queries (read-only operations)            │
│  • Handlers (business logic execution)       │
│  • DTOs (Data Transfer Objects)              │
│  • Validators (FluentValidation)             │
│  • AutoMapper Profiles                       │
└────────────────┬────────────────────────────┘
                 │ Domain Objects
┌────────────────▼────────────────────────────┐
│        Domain Layer (DDD)                    │
│  • Aggregate Roots                           │
│  • Entities                                  │
│  • Value Objects                             │
│  • Domain Events                             │
│  • Business Logic Invariants                 │
└────────────────┬────────────────────────────┘
                 │ Repository Interface
┌────────────────▼────────────────────────────┐
│     Infrastructure Layer                     │
│  • Repositories (Data Persistence)           │
│  • DbContext (Entity Framework Core)         │
│  • External Service Integration              │
│  • Event Publishing                          │
└────────────────┬────────────────────────────┘
                 │ Database/Services
```

## Authentication & Authorization Flow

```
Client Request
    │
    ▼
┌──────────────────────┐
│  API Gateway         │
│  (CORS Check)        │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Service Controller   │
│ [Authorize]          │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────────────┐
│ Extract JWT from Bearer Token │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────┐
│ Verify JWT Signature │
│ • Secret Key         │
│ • Expiration         │
│ • Issuer/Audience    │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Extract User Claims  │
│ • UserId             │
│ • Email              │
│ • Roles              │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Authorize Request    │
│ (Role Check)         │
└──────────┬───────────┘
           │
         ✅/❌
         │
    ✅   │  ❌
    │    │   │
    ▼    │   ▼
Access   │  Forbidden
    │    │   (403)
    ▼    ▼
  Continue Response
```

## Event-Driven Flow Example: User Registration

```
1. Register Request
   ┌────────────────────────┐
   │ POST /api/auth/register│
   │ {email, password, name}│
   └────────────┬───────────┘
                │ Validation
                ▼
   ┌────────────────────────┐
   │ RegisterCommand        │
   │ created                │
   └────────────┬───────────┘
                │ Handler
                ▼
   ┌────────────────────────┐
   │ RegisterCommandHandler │
   │ • Hash password        │
   │ • Create user          │
   │ • Publish event        │
   └────────────┬───────────┘
                │
    ┌───────────┴───────────┐
    │  UserRegistered Event │
    │  (Domain Event)        │
    └───────────┬───────────┘
                │ Published to
                ▼ Azure Service Bus
    
2. Services Subscribe & React
    
    ┌──────────────────────────────────┐
    │ UserProfile Service Subscriber   │
    │ • Receive UserRegistered event   │
    │ • Create default profile         │
    │ • Create preferences             │
    │ • Publish ProfileCreated event   │
    └──────────────────────────────────┘
    
    ┌──────────────────────────────────┐
    │ Notification Service Subscriber  │
    │ • Receive UserRegistered event   │
    │ • Send welcome email             │
    │ • Record in audit log            │
    └──────────────────────────────────┘

3. Complete
   ✅ User created & authenticated
   ✅ Profile auto-created
   ✅ Welcome email sent
```

## Data Flow for File Upload

```
Client
  │
  ├─ Multipart Form Data
  │  ├─ file (binary)
  │  ├─ category (string)
  │  └─ relatedEntityId (int)
  │
  ▼
┌──────────────────────────────┐
│ File Service API Controller  │
└──────────┬───────────────────┘
           │
           ▼ IFormFile
┌──────────────────────────────┐
│ UploadFileCommand Handler     │
│ • Validate file size (<50MB) │
│ • Validate MIME type         │
│ • Create FileRecord entity   │
└──────────┬───────────────────┘
           │
    ┌──────┴──────┐
    │ Sequential  │
    ▼             ▼
┌─────────────┐  ┌──────────────────┐
│ Virus Scan  │  │ Store File       │
│ (Mock for   │  │ • Local FS for   │
│  dev)       │  │   development    │
│             │  │ • Azure Blob for │
│             │  │   production     │
└────────┬────┘  └────────┬─────────┘
         │                │
    ✅/❌               ✅/❌
    │                  │
    └────────┬─────────┘
             │
             ▼
    ┌─────────────────────┐
    │ Save to Database    │
    │ • Store path        │
    │ • File metadata     │
    │ • User association  │
    └─────────┬───────────┘
              │
              ▼
    ┌─────────────────────┐
    │ Return Response     │
    │ • FileId            │
    │ • DownloadUrl       │
    │ • Status            │
    └─────────────────────┘
```

## CQRS Pattern Implementation

```
User Action / External Event
    │
    ├─ Mutating Operation (POST/PUT/DELETE)      ├─ Query Operation (GET)
    │                                             │
    ▼                                             ▼
┌─────────────────┐                     ┌──────────────────┐
│ Command Object  │                     │ Query Object     │
│ (immutable)     │                     │ (immutable)      │
└────────┬────────┘                     └────────┬─────────┘
         │                                       │
         ▼                                       ▼
┌─────────────────────────┐            ┌────────────────────┐
│ Command Handler         │            │ Query Handler      │
│ • Validate              │            │ • Fetch data       │
│ • Update state          │            │ • Apply filters    │
│ • Publish events        │            │ • Map to DTO       │
│ • Update database       │            │ • Return projection│
└────────┬────────────────┘            └────────┬───────────┘
         │                                       │
         ▼                                       ▼
    Updated State                         Database Projection
         │                                       │
         ├─ Event Published                     └─ Query Result
         │                                       
         ▼
    Other Services React
```

## Service Communication Patterns

### Synchronous (REST via API Gateway)
```
Service A → API Gateway → Service B
    │                         │
    ├─ HTTP Request          ├─ Deserialization
    ├─ JWT Token             ├─ Processing
    ├─ Wait for Response      ├─ Serialization
    └─ Response              └─ HTTP Response
```

### Asynchronous (Event-Driven)
```
Service A
    │
    ├─ Execute Action
    ├─ Publish Event (Fire & Forget)
    │
    └─ Continue Processing

Azure Service Bus
    │
    ├─ Route Event
    │
    ├─ Service B Subscriber ──→ Reaction B
    ├─ Service C Subscriber ──→ Reaction C
    └─ Service D Subscriber ──→ Reaction D
```

## Deployment Architecture

### Local Development
```
Docker Compose
    ├─ SQL Server (1433)
    ├─ RabbitMQ (5672)
    ├─ Identity Service
    ├─ Profile Service
    ├─ File Service
    ├─ Pet Service
    ├─ Notification Service
    └─ API Gateway
```

### Azure Production
```
Azure Kubernetes Service (AKS)
    ├─ Deployment: Identity Service (replicas: 3)
    ├─ Deployment: Profile Service (replicas: 2)
    ├─ Deployment: File Service (replicas: 2)
    ├─ Deployment: Pet Service (replicas: 3)
    ├─ Deployment: Notification Service (replicas: 2)
    ├─ Deployment: API Gateway (replicas: 2)
    ├─ Service: Load Balancer
    └─ Ingress: TLS termination

Azure SQL Database (Premium tier, geo-redundant)
    ├─ IdentityServiceDb
    ├─ UserProfileServiceDb
    ├─ PetServiceDb
    └─ FileServiceDb

Azure Service Bus (Standard+)
    ├─ Topic: integration-events
    └─ Subscriptions per service

Azure Container Registry
    └─ pet-app/identity-service:v1.0.0
    └─ pet-app/profile-service:v1.0.0
    └─ etc.

Azure Key Vault
    ├─ JwtSettings--SecretKey
    ├─ ConnectionStrings--IdentityServiceDb
    └─ etc.
```

## Technology Layers

### Transport Layer
- HTTP/HTTPS via Kestrel
- CORS for cross-origin requests
- Content negotiation

### API Layer
- RESTful endpoints
- OpenAPI/Swagger documentation
- Request/Response DTOs
- Middleware pipeline

### Application Layer
- Command/Query handlers (MediatR)
- Business logic orchestration
- Transaction management
- Validation & error handling

### Domain Layer
- Aggregate roots with invariants
- Value objects (immutable)
- Domain events
- No external dependencies

### Data Access Layer
- Entity Framework Core with EF Core
- Repository pattern
- Unit of Work pattern
- Query optimization

### External Integration Layer
- OAuth providers (Google, Facebook, Apple)
- Email service (SendGrid/mock)
- SMS service (Twilio/mock)
- File storage (Azure Blob/local filesystem)
- Message bus (Azure Service Bus/RabbitMQ)

## Error Handling Strategy

```
Exception Occurs
    │
    ▼
┌─────────────────────────────┐
│ Determine Exception Type    │
└─────────┬───────────────────┘
          │
    ┌─────┼─────┬──────────┬─────────────┐
    │     │     │          │             │
    ▼     ▼     ▼          ▼             ▼
Domain  Validation Business HTTP        Unexpected
Exception Exception  Logic Exception    Exception
    │     │     │          │             │
    ▼     ▼     ▼          ▼             ▼
   400   400   409        4xx          500
BadReq  BadReq Conflict   Varies     Internal
        Error
    │     │     │          │             │
    └─────┴─────┴──────────┴─────────────┘
              │
              ▼ Mapped to
    ┌──────────────────────┐
    │ ProblemDetails (RFC) │
    │ • Type               │
    │ • Title              │
    │ • Status             │
    │ • Detail             │
    │ • TraceId            │
    └──────────┬───────────┘
               │
               ▼ Logged & Returned
         HTTP Response
```

## Caching Strategy (Future Enhancement)

```
Request
    │
    ▼
┌──────────────┐
│ Redis Cache  │ ──✅──→ Cache Hit → Return
└──────┬───────┘
       │ ❌ Miss
       ▼
┌──────────────────┐
│ Database/Service │
└──────┬───────────┘
       │
       ▼
┌──────────────┐
│ Update Cache │
└──────┬───────┘
       │
       ▼
    Response
```

## Key Design Decisions

1. **Per-Service Database**: No shared database to maintain service independence
2. **Event-Driven**: Loose coupling between services via async messaging
3. **DDD**: Rich domain models with business logic enforcement
4. **CQRS**: Separate read/write models for better scalability
5. **Repository Pattern**: Consistent data access abstraction
6. **Dependency Injection**: Full container support for testability
7. **Async/Await**: Non-blocking I/O throughout
8. **JWT Tokens**: Stateless authentication without session storage

## Scalability Considerations

### Horizontal Scaling
- Stateless services deployable to multiple instances
- Load balancer distributes traffic
- Session data in Database/Cache, not memory

### Vertical Scaling
- Connection pooling optimized
- Query performance tuned with indices
- Cache for frequently accessed data

### Database Scaling
- Read replicas for reporting
- Sharding for large datasets
- Archive old data

---

**This architecture is designed for:**
- ✅ High availability
- ✅ Easy scaling
- ✅ Service independence
- ✅ Technology flexibility
- ✅ Team autonomy
- ✅ Simplified testing
- ✅ Gradual rollouts
- ✅ Resilience to failures

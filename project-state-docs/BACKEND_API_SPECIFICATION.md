# Pet App Backend - API Specification
**Date:** 2026-03-27  
**Backend Stack:** .NET 10, DDD Architecture, Microservices

## Overview

The Pet App Backend is a comprehensive microservices architecture with 5 main services, each serving specific domain purposes. Services communicate via REST APIs and async events through Azure Service Bus (or RabbitMQ locally).

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    API Gateway (Port 44300)                  │
└─────────────┬───────────────────────────────────────────────┘
              │
    ┌─────────┼─────────────────────────┬──────────────┐
    │         │                         │              │
    ▼         ▼                         ▼              ▼
┌────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐ ┌──────────┐
│Identity│ │User Prof │ │   Pet    │ │ File   │ │Notific.  │
│Service │ │ Service  │ │ Service  │ │Service │ │ Service  │
│44301   │ │  44302   │ │  5000    │ │ 44303  │ │ 44304    │
└────────┘ └──────────┘ └──────────┘ └────────┘ └──────────┘
    │          │            │            │          │
    └──────────┴────────────┴────────────┴──────────┘
               │
          ┌────▼─────────────────┐
          │  PostgreSQL (Multi-DB)
          │  Event Bus (RabbitMQ)
          │  Azure Blob Storage
          └──────────────────────┘
```

## Service Ports

| Service | Port | Protocol | Type |
|---------|------|----------|------|
| API Gateway | 44300 | HTTPS | Main Entry Point |
| Identity Service | 44301 | HTTPS | Authentication & Auth |
| User Profile Service | 44302 | HTTPS | User Info Management |
| Pet Service | 5000 | HTTP | Pet Data Management |
| File Service | 44303 | HTTPS | File Upload/Download |
| Notification Service | 44304 | HTTPS | Email/SMS Notifications |

## Authentication

All endpoints require **JWT Bearer Token** (except public auth endpoints).

### Token Structure
- **Type**: JWT (JSON Web Token)
- **Signing**: HS256 (HMAC with SHA-256)
- **Issuer**: `pet-app-identity-service`
- **Audience**: `pet-app-api`
- **Access Token Expiry**: 15 minutes
- **Refresh Token Expiry**: 7 days (10080 minutes)

### Using Tokens
```bash
# Add to request header
Authorization: Bearer <access_token>
```

---

## Service APIs

### 1. Identity Service (Port 44301)

**Purpose**: User registration, login, JWT token management, OAuth provider integration

#### Endpoints

##### Public (No Auth Required)

**POST** `/api/auth/register`
- Register new user with email/password
- Request:
  ```json
  {
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "fullName": "John Doe"
  }
  ```
- Response: `AuthenticationResponseDto` (200 OK, 400 BadRequest, 409 Conflict)

**POST** `/api/auth/login`
- Login with email/password
- Request:
  ```json
  {
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }
  ```
- Response: `AuthenticationResponseDto` (200 OK, 401 Unauthorized)

**POST** `/api/auth/oauth-login`
- OAuth authentication (Google, Facebook, Apple)
- Request:
  ```json
  {
    "provider": "google",  // "google", "facebook", "apple"
    "code": "auth_code_from_provider",
    "idToken": "optional_id_token_for_apple"
  }
  ```
- Response: `AuthenticationResponseDto` (200 OK, 400 BadRequest)

**POST** `/api/auth/refresh-token`
- Refresh expired access token
- Request:
  ```json
  {
    "refreshToken": "refresh_token_value"
  }
  ```
- Response: `AuthenticationResponseDto` (200 OK, 401 Unauthorized)

**POST** `/api/auth/check-email`
- Check if email exists (public)
- Request: `{ "email": "user@example.com" }`
- Response: `{ "exists": true }` (200 OK)

##### Protected (Auth Required)

**GET** `/api/auth/profile`
- Get current user profile
- Response: `UserDto` (200 OK, 401 Unauthorized)

**PUT** `/api/auth/profile`
- Update user profile
- Request:
  ```json
  {
    "fullName": "Jane Doe",
    "avatar": "https://example.com/avatar.jpg"
  }
  ```
- Response: `UserDto` (200 OK, 401 Unauthorized)

**POST** `/api/auth/change-password`
- Change password
- Request:
  ```json
  {
    "currentPassword": "OldPass123!",
    "newPassword": "NewPass123!"
  }
  ```
- Response: 204 NoContent (401 Unauthorized, 400 BadRequest)

**POST** `/api/auth/link-oauth`
- Link OAuth provider to existing account
- Request:
  ```json
  {
    "provider": "google",
    "providerUserId": "google_user_id",
    "providerEmail": "user@gmail.com"
  }
  ```
- Response: `UserDto` (200 OK)

**POST** `/api/auth/unlink-oauth`
- Unlink OAuth provider
- Request: `{ "provider": "google" }`
- Response: `UserDto` (200 OK)

**POST** `/api/auth/logout`
- Logout (revoke all refresh tokens)
- Response: 204 NoContent (401 Unauthorized)

**POST** `/api/auth/deactivate`
- Deactivate account
- Response: 204 NoContent (401 Unauthorized)

#### Data Models

**UserDto**
```typescript
{
  id: UUID,
  email: string,
  fullName?: string,
  avatar?: string,
  isEmailVerified: boolean,
  isActive: boolean,
  createdAt: datetime,
  updatedAt?: datetime,
  lastLoginAt?: datetime,
  oAuthLinks: OAuthLinkDto[]
}
```

**OAuthLinkDto**
```typescript
{
  provider: string,  // "google", "facebook", "apple"
  linkedAt: datetime
}
```

**AuthenticationResponseDto**
```typescript
{
  user: UserDto,
  accessToken: string,
  refreshToken: string,
  expiresAt: datetime
}
```

---

### 2. User Profile Service (Port 44302)

**Purpose**: Manage user profiles, preferences, notification settings, language/timezone

#### Endpoints

All endpoints require authentication.

**GET** `/api/profile/{userProfileId}`
- Get user profile by ID
- Response: `UserProfileDto` (200 OK, 404 NotFound)

**GET** `/api/profile/by-user/{userId}`
- Get user profile by user ID
- Response: `UserProfileDto` (200 OK, 404 NotFound)

**PUT** `/api/profile/{userProfileId}`
- Update user profile
- Request:
  ```json
  {
    "firstName": "John",
    "lastName": "Doe",
    "bio": "Pet lover",
    "dateOfBirth": "1990-01-15",
    "phoneNumber": "+1234567890",
    "address": "123 Main St",
    "city": "New York",
    "country": "USA",
    "profilePictureUrl": "https://example.com/pic.jpg"
  }
  ```
- Response: `UserProfileDto` (200 OK, 400 BadRequest)

**GET** `/api/profile/{userProfileId}/notifications`
- Get notification preferences
- Response: `NotificationPreferencesDto` (200 OK, 404 NotFound)

**PUT** `/api/profile/{userProfileId}/notifications`
- Update notification preferences
- Request:
  ```json
  {
    "emailNotifications": true,
    "pushNotifications": true,
    "smsNotifications": false,
    "receivePromotions": true,
    "receiveNewsletter": true
  }
  ```
- Response: `NotificationPreferencesDto` (200 OK)

**PUT** `/api/profile/{userProfileId}/language`
- Update language and timezone
- Request:
  ```json
  {
    "language": "en",
    "timezone": "America/New_York"
  }
  ```
- Response: 204 NoContent (200 OK in latest version)

#### Data Models

**UserProfileDto**
```typescript
{
  id: UUID,
  userId: UUID,
  firstName: string,
  lastName: string,
  bio?: string,
  dateOfBirth?: datetime,
  phoneNumber?: string,
  address?: string,
  city?: string,
  country?: string,
  profilePictureUrl?: string,
  language: string,
  timezone: string,
  createdAt: datetime,
  updatedAt?: datetime
}
```

**NotificationPreferencesDto**
```typescript
{
  id: UUID,
  emailNotifications: boolean,
  pushNotifications: boolean,
  smsNotifications: boolean,
  receivePromotions: boolean,
  receiveNewsletter: boolean,
  updatedAt?: datetime
}
```

---

### 3. Pet Service (Port 5000)

**Purpose**: Manage pet records, photos, medical documents, vaccinations

#### Pet Management Endpoints

**POST** `/api/pets?ownerId={userId}`
- Create new pet
- Request:
  ```json
  {
    "name": "Buddy",
    "type": "Dog",
    "breed": "Golden Retriever",
    "dateOfBirth": "2020-05-15",
    "description": "Friendly and energetic"
  }
  ```
- Response: `PetDto` (201 Created, 400 BadRequest)

**GET** `/api/pets?ownerId={userId}&page=1&pageSize=10`
- Get all pets for owner (paginated)
- Response: `PaginatedResponse<PetDto>` (200 OK)

**GET** `/api/pets/{petId}?ownerId={userId}`
- Get specific pet by ID
- Response: `PetDto` (200 OK, 404 NotFound)

**GET** `/api/pets/search?ownerId={userId}&searchTerm=name&page=1&pageSize=10`
- Search pets by name
- Response: `PaginatedResponse<PetDto>` (200 OK)

**PUT** `/api/pets/{petId}?ownerId={userId}`
- Update pet information
- Request:
  ```json
  {
    "name": "Buddy",
    "breed": "Golden Retriever",
    "description": "Friendly and energetic"
  }
  ```
- Response: `PetDto` (200 OK, 404 NotFound)

**DELETE** `/api/pets/{petId}?ownerId={userId}`
- Delete pet
- Response: 204 NoContent (404 NotFound)

#### Pet Photos Endpoints

**GET** `/api/pets/{petId}/photos?ownerId={userId}`
- Get all photos for a pet
- Response: `PhotoDto[]` (200 OK, 404 NotFound)

**GET** `/api/pets/{petId}/photos/{photoId}?ownerId={userId}`
- Get specific photo
- Response: `PhotoDto` (200 OK, 404 NotFound)

**POST** `/api/pets/{petId}/photos?ownerId={userId}`
- Add photo to pet
- Request:
  ```json
  {
    "fileName": "buddy_photo.jpg",
    "fileType": "image/jpeg",
    "fileSizeBytes": 2048000,
    "url": "https://storage.example.com/photos/buddy_photo.jpg",
    "caption": "Buddy at the park",
    "tags": "outdoor, happy, summer"
  }
  ```
- Response: `PhotoDto` (201 Created, 404 NotFound)

**PUT** `/api/pets/{petId}/photos/{photoId}?ownerId={userId}`
- Update photo metadata
- Request:
  ```json
  {
    "caption": "Updated caption",
    "tags": "updated, tags"
  }
  ```
- Response: `PhotoDto` (200 OK, 404 NotFound)

**DELETE** `/api/pets/{petId}/photos/{photoId}?ownerId={userId}`
- Delete photo
- Response: 204 NoContent (404 NotFound)

#### Pet Documents Endpoints

**GET** `/api/pets/{petId}/documents?ownerId={userId}`
- Get all documents for a pet
- Response: `DocumentDto[]` (200 OK, 404 NotFound)

**GET** `/api/pets/{petId}/documents/{docId}?ownerId={userId}`
- Get specific document
- Response: `DocumentDto` (200 OK, 404 NotFound)

**POST** `/api/pets/{petId}/documents?ownerId={userId}`
- Add document (vaccination, medical record, etc.)
- Request:
  ```json
  {
    "fileName": "vaccination_record.pdf",
    "fileType": "application/pdf",
    "fileSizeBytes": 102400,
    "url": "https://storage.example.com/docs/vaccination.pdf",
    "category": "vaccination",
    "description": "Annual vaccination 2024"
  }
  ```
- Response: `DocumentDto` (201 Created, 404 NotFound)

**PUT** `/api/pets/{petId}/documents/{docId}?ownerId={userId}`
- Update document metadata
- Request: `{ "description": "Updated description" }`
- Response: `DocumentDto` (200 OK, 404 NotFound)

**DELETE** `/api/pets/{petId}/documents/{docId}?ownerId={userId}`
- Delete document
- Response: 204 NoContent (404 NotFound)

#### Data Models

**PetDto**
```typescript
{
  id: UUID,
  ownerId: UUID,
  name: string,
  type: string,  // "Dog", "Cat", "Bird", etc.
  breed?: string,
  dateOfBirth: datetime,
  description?: string,
  createdAt: datetime,
  updatedAt?: datetime,
  photos: PhotoDto[],
  documents: DocumentDto[]
}
```

**PhotoDto**
```typescript
{
  id: UUID,
  fileName: string,
  fileType: string,
  fileSizeBytes: number,
  url: string,
  uploadedAt: datetime,
  caption?: string,
  tags?: string
}
```

**DocumentDto**
```typescript
{
  id: UUID,
  fileName: string,
  fileType: string,
  fileSizeBytes: number,
  url: string,
  category: string,  // "vaccination", "medical", "insurance", etc.
  uploadedAt: datetime,
  description?: string
}
```

**PaginatedResponse<T>**
```typescript
{
  items: T[],
  pageNumber: number,
  pageSize: number,
  totalCount: number,
  totalPages: number,
  hasPreviousPage: boolean,
  hasNextPage: boolean
}
```

---

### 4. File Service (Port 44303)

**Purpose**: Centralized file upload/download, virus scanning, signed URLs, blob storage

#### Endpoints

All endpoints require authentication.

**POST** `/api/files/upload`
- Upload file (multipart form-data)
- Form Parameters:
  - `file`: File to upload (required)
  - `category`: File category (optional, e.g., "pet_photo", "medical_record")
  - `relatedEntityId`: UUID of related entity like Pet ID (optional)
- Response: `FileDto` (200 OK, 400 BadRequest)

**GET** `/api/files/{fileId}`
- Get file metadata
- Response: `FileDto` (200 OK, 404 NotFound)

**GET** `/api/files/{fileId}/download-url?expirationMinutes=60`
- Get signed download URL (expires in specified minutes)
- Response: `{ "downloadUrl": "https://..." }` (200 OK)

**DELETE** `/api/files/{fileId}`
- Delete file
- Response: 204 NoContent (401 Unauthorized, 404 NotFound)

**GET** `/api/files/user/files?pageNumber=1&pageSize=10`
- List all user's files (paginated)
- Response: `PaginatedListDto<FileDto>` (200 OK)

**GET** `/api/files/entity/{relatedEntityId}/files?pageNumber=1&pageSize=10`
- List files related to specific entity (e.g., all photos for a pet)
- Response: `PaginatedListDto<FileDto>` (200 OK)

#### Data Models

**FileDto**
```typescript
{
  id: UUID,
  fileName: string,
  contentType: string,
  fileSizeBytes: number,
  category?: string,
  relatedEntityId?: UUID,
  userId: UUID,
  uploadedAt: datetime,
  virusScanStatus: string,  // "pending", "clean", "infected"
  storageUrl: string
}
```

**PaginatedListDto<T>**
```typescript
{
  items: T[],
  pageNumber: number,
  pageSize: number,
  totalCount: number,
  totalPages: number
}
```

---

### 5. Notification Service (Port 44304)

**Purpose**: Send email and SMS notifications, event-driven notifications from other services

#### Endpoints

**POST** `/api/notifications/email`
- Send email notification
- Request:
  ```json
  {
    "to": "user@example.com",
    "subject": "Notification Subject",
    "body": "Email body content",
    "htmlBody": "<h1>HTML content</h1>"
  }
  ```
- Response: 200 OK, 400 BadRequest

**POST** `/api/notifications/sms`
- Send SMS notification (if configured)
- Request:
  ```json
  {
    "phoneNumber": "+1234567890",
    "message": "SMS content"
  }
  ```
- Response: 200 OK, 400 BadRequest

**GET** `/api/notifications/history`
- Get notification history
- Response: List of notifications (200 OK)

#### Event-Driven Notifications

Services emit events that automatically trigger notifications:
- **User Registration**: Welcome email sent
- **Password Change**: Confirmation email sent
- **Pet Created**: Notification sent to user

---

## Common Response Patterns

### Success Response
```json
{
  "id": "uuid",
  "data": "response data"
}
```

### Error Response (400 Bad Request)
```json
{
  "error": "Error message",
  "code": "ERROR_CODE",
  "errors": {
    "fieldName": ["Validation error message"]
  }
}
```

### Error Response (404 Not Found)
```json
{
  "error": "Resource not found",
  "code": "NOT_FOUND"
}
```

### Error Response (401 Unauthorized)
```json
{
  "error": "Invalid or missing authentication token",
  "code": "UNAUTHORIZED"
}
```

---

## Configuration

### Database Connections
All services use separate PostgreSQL databases:
- `PetServiceDb` - Pet service data
- `PetApp.IdentityService` - Auth data
- `PetApp.UserProfileService` - User profiles
- `PetApp.FileService` - File metadata
- `PetApp.NotificationService` - Notification logs

### JWT Configuration
Set in environment variables:
```
JwtSettings__SecretKey=your-min-32-char-secret-key-here
JwtSettings__Issuer=pet-app-identity-service
JwtSettings__Audience=pet-app-api
JwtSettings__AccessTokenExpirationMinutes=15
JwtSettings__RefreshTokenExpirationMinutes=10080
```

### Event Bus
- **Development**: RabbitMQ on `amqp://guest:guest@localhost:5672/`
- **Production**: Azure Service Bus with connection string

---

## Deployment & Local Development

### Docker Compose
```bash
docker-compose up --build
```

### Manual Service Startup
Each service has its own startup:
```bash
# Build all
dotnet build

# Run Identity Service
cd src/services/identity-service/src/IdentityService.Api
dotnet run

# Run Pet Service
cd src/services/pet-service/src/PetService.Api
dotnet run

# Run other services similarly
```

### API Documentation
Each service exposes Swagger UI at:
- Identity: https://localhost:44301/swagger
- User Profile: https://localhost:44302/swagger
- Pet: http://localhost:5000/swagger
- File: https://localhost:44303/swagger
- Notification: https://localhost:44304/swagger

---

## Key Features & Patterns

- **DDD**: Domain-driven design with aggregates, value objects, domain events
- **CQRS**: Command Query Responsibility Segregation pattern
- **MediatR**: In-process messaging for commands and queries
- **Repository Pattern**: Data access abstraction
- **Async/Await**: Fully async APIs
- **Logging**: Structured logging with Serilog
- **Error Handling**: Domain exceptions, validation errors, proper HTTP status codes
- **Security**: JWT authentication, CORS, HTTPS enforcement

---

## Important Notes for Frontend

1. **All requests** (except public auth endpoints) require `Authorization: Bearer <token>` header
2. **CORS** is configured for frontend domain (check appsettings)
3. **Tokens expire** in 15 minutes - use refresh-token endpoint to get new ones
4. **Owner ID required**: Most Pet Service endpoints need `ownerId` query parameter
5. **File uploads**: Use multipart/form-data, not JSON
6. **Pagination**: Default pageSize is 10, max depends on service
7. **Error codes**: Check `code` field for specific error handling
8. **Timestamps**: All dates are ISO 8601 format in UTC

---

**Last Updated:** 2026-03-27  
**Backend Repository**: pet-app-backend

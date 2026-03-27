# Backend Architecture Analysis - Pet App Backend
**Date:** 2026-03-27

## Backend Overview

**Technology Stack**: .NET 10, DDD Architecture, Microservices Pattern  
**API Style**: RESTful with JSON  
**Authentication**: JWT (15 min access tokens, 7-day refresh tokens)  
**Communication**: REST APIs + Event-driven via RabbitMQ/Azure Service Bus  
**Databases**: PostgreSQL (multi-database per service pattern)  

## 5 Microservices

### 1. **Identity Service** (Port 44301)
- **Purpose**: User authentication, JWT management, OAuth integration
- **Key Endpoints**:
  - `POST /api/auth/register` - Register new user
  - `POST /api/auth/login` - Login with email/password
  - `POST /api/auth/oauth-login` - OAuth (Google, Facebook, Apple)
  - `GET /api/auth/profile` - Get current user
  - `POST /api/auth/refresh-token` - Refresh expired token
  - `POST /api/auth/logout` - Logout & revoke tokens
  - `POST /api/auth/change-password` - Change password
  - `POST /api/auth/link-oauth` - Connect OAuth to account
  - `POST /api/auth/unlink-oauth` - Disconnect OAuth

- **Response Model**: `AuthenticationResponseDto`
  ```typescript
  {
    user: { id, email, fullName, avatar, isEmailVerified, isActive, oAuthLinks[] },
    accessToken: string,
    refreshToken: string,
    expiresAt: datetime
  }
  ```

### 2. **User Profile Service** (Port 44302)
- **Purpose**: User profile & preference management
- **Key Endpoints**:
  - `GET /api/profile/{userProfileId}` - Get profile
  - `GET /api/profile/by-user/{userId}` - Get by user ID
  - `PUT /api/profile/{userProfileId}` - Update profile (name, bio, address, etc.)
  - `GET /api/profile/{userProfileId}/notifications` - Get notification preferences
  - `PUT /api/profile/{userProfileId}/notifications` - Update notification settings
  - `PUT /api/profile/{userProfileId}/language` - Set language & timezone

- **Response Model**: `UserProfileDto`
  ```typescript
  {
    id, userId, firstName, lastName, bio, dateOfBirth, 
    phoneNumber, address, city, country, profilePictureUrl,
    language, timezone, createdAt, updatedAt
  }
  ```

### 3. **Pet Service** (Port 5000)
- **Purpose**: Pet record management, photos, medical documents
- **Key Endpoints**:
  - **Pet CRUD**:
    - `POST /api/pets?ownerId={id}` - Create pet
    - `GET /api/pets?ownerId={id}&page=1&pageSize=10` - List pets (paginated)
    - `GET /api/pets/{petId}?ownerId={id}` - Get single pet
    - `GET /api/pets/search?ownerId={id}&searchTerm=name` - Search pets
    - `PUT /api/pets/{petId}?ownerId={id}` - Update pet
    - `DELETE /api/pets/{petId}?ownerId={id}` - Delete pet

  - **Pet Photos**:
    - `GET /api/pets/{petId}/photos?ownerId={id}` - List photos
    - `GET /api/pets/{petId}/photos/{photoId}?ownerId={id}` - Get photo
    - `POST /api/pets/{petId}/photos?ownerId={id}` - Add photo
    - `PUT /api/pets/{petId}/photos/{photoId}?ownerId={id}` - Update photo
    - `DELETE /api/pets/{petId}/photos/{photoId}?ownerId={id}` - Delete photo

  - **Pet Documents** (vaccination, medical records):
    - `GET /api/pets/{petId}/documents?ownerId={id}` - List documents
    - `POST /api/pets/{petId}/documents?ownerId={id}` - Add document
    - `PUT /api/pets/{petId}/documents/{docId}?ownerId={id}` - Update document
    - `DELETE /api/pets/{petId}/documents/{docId}?ownerId={id}` - Delete document

- **Response Models**:
  ```typescript
  PetDto: { id, ownerId, name, type, breed, dateOfBirth, description, createdAt, updatedAt, photos[], documents[] }
  PhotoDto: { id, fileName, fileType, fileSizeBytes, url, uploadedAt, caption, tags }
  DocumentDto: { id, fileName, fileType, fileSizeBytes, url, category, uploadedAt, description }
  PaginatedResponse: { items[], pageNumber, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage }
  ```

### 4. **File Service** (Port 44303)
- **Purpose**: File uploads/downloads with virus scanning, signed URLs
- **Key Endpoints**:
  - `POST /api/files/upload` - Upload file (multipart form-data)
  - `GET /api/files/{fileId}` - Get file metadata
  - `GET /api/files/{fileId}/download-url?expirationMinutes=60` - Get signed download URL
  - `DELETE /api/files/{fileId}` - Delete file
  - `GET /api/files/user/files?pageNumber=1&pageSize=10` - List user's files
  - `GET /api/files/entity/{relatedEntityId}/files` - List files for entity (e.g., pet)

- **Response Model**: `FileDto`
  ```typescript
  { id, fileName, contentType, fileSizeBytes, category, relatedEntityId, userId, uploadedAt, virusScanStatus, storageUrl }
  ```

### 5. **Notification Service** (Port 44304)
- **Purpose**: Email/SMS notifications, event-driven notifications
- **Key Endpoints**:
  - `POST /api/notifications/email` - Send email
  - `POST /api/notifications/sms` - Send SMS
  - `GET /api/notifications/history` - Get notification history

- **Event-driven**: Auto-sends on user registration, password change, pet created, etc.

---

## Critical Details for Frontend Integration

### Authentication Flow
1. **Register/Login** → Get JWT (access token + refresh token)
2. **Store tokens** → Securely in device (AsyncStorage with encryption recommended)
3. **Use token** → Add `Authorization: Bearer <token>` to every request
4. **On 401** → Call refresh-token endpoint, get new token, retry request
5. **Tokens expire** → 15 minutes (access), 7 days (refresh)

### Important Query Parameters
- **All Pet Service endpoints require `ownerId` query param** (user's UUID)
- **Pagination**: `page` (Pet) or `pageNumber` (File), `pageSize`
- **Search**: `/api/pets/search?ownerId={id}&searchTerm=query`

### File Uploads
- Use `multipart/form-data`, not JSON
- Optional fields: `category`, `relatedEntityId`
- Returns signed download URL with expiration

### Error Handling
All errors follow pattern:
```json
{
  "error": "Description",
  "code": "ERROR_CODE"
}
```
Common codes: `NOT_FOUND`, `VALIDATION_ERROR`, `UNAUTHORIZED`, `FORBIDDEN`

### Environments
- **Local Dev**: Services on localhost:44300-44304, RabbitMQ
- **Docker Compose**: All services + PostgreSQL + RabbitMQ
- **Production**: HTTPS only, Azure Service Bus, signed URLs

---

## Architecture Principles Used

1. **Domain-Driven Design (DDD)**: Services organized around business domains
2. **CQRS Pattern**: Separate read (queries) and write (commands) operations
3. **Repository Pattern**: Data access abstraction
4. **MediatR**: In-process messaging for commands/queries
5. **Event Sourcing**: Async communication between services
6. **Clean Architecture**: Clear separation of concerns

---

## Key Implementation Notes

### Token Management
- Access token: 15 minutes (short-lived for security)
- Refresh token: 7 days (long-lived for convenience)
- Revocation on logout: All refresh tokens invalidated
- OAuth support: Google, Facebook, Apple

### Database
- 5 separate PostgreSQL databases (database per service pattern)
- Enables independent scaling and schema evolution
- Local dev uses default user `postgres:PetApp123!@#`

### Security
- HTTPS enforced (except Pet Service on 5000 for local dev)
- CORS configured
- Password requirements enforced
- JWT signing with 256-bit keys
- Email verification support
- OAuth provider integration

### Storage
- Azure Blob Storage or local filesystem
- Virus scanning on uploads
- Signed download URLs with expiration
- File association with users and entities

### Notifications
- Email: SendGrid provider
- SMS: Configurable provider
- Event-driven: Published when key actions occur
- Notification preferences per user

---

## Frontend Development Priorities

**Phase 1 - Essential**:
1. HTTP client with auth token handling
2. Login/Register screens
3. Pet list screen
4. Pet CRUD operations

**Phase 2 - Core Features**:
1. User profile management
2. Pet photos/documents
3. Logout functionality
4. Token refresh logic

**Phase 3 - Polish**:
1. Error notifications
2. Loading states
3. Image uploads with file service
4. Search functionality

---

## Connection Details for Frontend

```
Frontend (pet-app)
    ↓
API Gateway: https://localhost:44300  (or production URL)
    ↓
Route to appropriate service:
  - /auth/* → Identity Service :44301
  - /profile/* → User Profile Service :44302
  - /pets/* → Pet Service :5000
  - /files/* → File Service :44303
  - /notifications/* → Notification Service :44304
```

For local dev: Point to `http://localhost:44300` (API Gateway)  
For production: Use actual domain + port configuration

---

## Testing the APIs

### Swagger/OpenAPI Docs Available at:
- https://localhost:44301/swagger (Identity)
- https://localhost:44302/swagger (User Profile)
- http://localhost:5000/swagger (Pet)
- https://localhost:44303/swagger (File)
- https://localhost:44304/swagger (Notification)

### Test Flow
```bash
# 1. Register/Login to get token
curl -X POST https://localhost:44301/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "Pass123!", "fullName": "Test User"}'

# 2. Use token in subsequent requests
curl -X GET https://localhost:44300/api/pets?ownerId=<user-id>&page=1 \
  -H "Authorization: Bearer <access_token>"
```

---

**Complete API Specification**: See `BACKEND_API_SPECIFICATION.md` for full endpoint details, request/response schemas, and error codes.

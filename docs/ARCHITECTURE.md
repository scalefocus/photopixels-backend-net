# Architecture Guide

## Table of Contents
1. [Overview](#overview)
2. [Clean Architecture](#clean-architecture)
3. [CQRS Pattern](#cqrs-pattern)
4. [Design Patterns](#design-patterns)
5. [Project Structure](#project-structure)
6. [Layer Details](#layer-details)
7. [Data Flow](#data-flow)

---

## Overview

PhotoPixels follows **Clean Architecture** principles with clear separation of concerns across four layers. This architecture ensures:
- **Maintainability**: Easy to understand and modify
- **Testability**: Business logic can be tested in isolation
- **Scalability**: New features can be added without affecting existing code
- **Flexibility**: Infrastructure can be swapped without changing business logic

### Architectural Principles

1. **Dependency Rule**: Dependencies point inward, toward the domain
2. **Separation of Concerns**: Each layer has a single, well-defined responsibility
3. **Dependency Inversion**: Depend on abstractions, not concretions
4. **SOLID Principles**: Applied throughout the codebase

---

## Clean Architecture

### The Four Layers

```
┌─────────────────────────────────────────────────────────┐
│              SF.PhotoPixels.API                         │
│         (Presentation / API Layer)                      │
│  • HTTP Endpoints                                       │
│  • Request/Response DTOs                                │
│  • Authentication/Authorization                         │
│  • Swagger Documentation                                │
└─────────────────────────────────────────────────────────┘
                        ↓ depends on
┌─────────────────────────────────────────────────────────┐
│           SF.PhotoPixels.Application                    │
│            (Application Layer)                          │
│  • Use Cases (Commands & Queries)                       │
│  • Business Workflows                                   │
│  • Validation Logic                                     │
│  • Application Services                                 │
└─────────────────────────────────────────────────────────┘
                        ↓ depends on
┌─────────────────────────────────────────────────────────┐
│             SF.PhotoPixels.Domain                       │
│              (Domain Layer)                             │
│  • Entities                                             │
│  • Domain Events                                        │
│  • Business Rules                                       │
│  • Repository Interfaces                                │
└─────────────────────────────────────────────────────────┘
                        ↑ implemented by
┌─────────────────────────────────────────────────────────┐
│          SF.PhotoPixels.Infrastructure                  │
│           (Infrastructure Layer)                        │
│  • Data Persistence (Marten/PostgreSQL)                 │
│  • External Services                                    │
│  • File Storage                                         │
│  • Email Service                                        │
└─────────────────────────────────────────────────────────┘
```

### Dependency Flow

**The Golden Rule**: Dependencies always point inward

- ✅ **API** depends on **Application**
- ✅ **Application** depends on **Domain**
- ✅ **Infrastructure** implements **Domain** interfaces
- ❌ **Domain** NEVER depends on **Infrastructure** or **API**

### Benefits of Clean Architecture

1. **Independent of Frameworks**: Business logic doesn't depend on external libraries
2. **Testable**: Business rules can be tested without UI, database, or external services
3. **Independent of UI**: UI can change without affecting business logic
4. **Independent of Database**: Can swap PostgreSQL for another database
5. **Independent of External Services**: Business logic doesn't know about external APIs

---

## CQRS Pattern

PhotoPixels uses **Command Query Responsibility Segregation (CQRS)** to separate read and write operations.

### Commands (Write Operations)

Commands **mutate state** and return void or a simple result:

```csharp
// Command
public class CreateAlbumRequest : IRequest<CreateAlbumResponse>
{
    public string Name { get; set; }
    public string? Description { get; set; }
}

// Handler
public class CreateAlbumHandler : IRequestHandler<CreateAlbumRequest, CreateAlbumResponse>
{
    public async ValueTask<CreateAlbumResponse> Handle(
        CreateAlbumRequest request, 
        CancellationToken cancellationToken)
    {
        // Mutate state
        var album = new Album(request.Name, request.Description);
        _session.Insert(album);
        await _session.SaveChangesAsync(cancellationToken);
        
        return new CreateAlbumResponse { AlbumId = album.Id };
    }
}
```

**Examples**:
- `CreateUserRequest`
- `DeleteUserRequest`
- `AdjustQuotaRequest`
- `TrashObjectRequest`
- `UploadChunkRequest`

### Queries (Read Operations)

Queries **read data** and return DTOs without modifying state:

```csharp
// Query
public class GetAlbumsRequest : IRequest<GetAlbumsResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// Handler
public class GetAlbumsHandler : IQueryHandler<GetAlbumsRequest, GetAlbumsResponse>
{
    public async ValueTask<GetAlbumsResponse> Handle(
        GetAlbumsRequest request, 
        CancellationToken cancellationToken)
    {
        // Read data only
        var albums = await _session.Query<Album>()
            .Where(x => x.UserId == _userId)
            .ToListAsync(cancellationToken);
        
        return new GetAlbumsResponse { Albums = albums };
    }
}
```

**Examples**:
- `GetObjectDataRequest`
- `GetAlbumsRequest`
- `GetObjectsListRequest`
- `LoadMediaRequest`

### CQRS Benefits

1. **Separation of Concerns**: Read and write logic are independent
2. **Performance Optimization**: Can optimize queries separately from commands
3. **Scalability**: Read and write databases can be scaled independently
4. **Simplified Logic**: Each handler has a single responsibility
5. **Better Testing**: Commands and queries can be tested separately

---

## Design Patterns

### 1. Mediator Pattern

**Purpose**: Decouples request senders from handlers

**Implementation**: Using the `Mediator` library (not MediatR)

```csharp
// Request
public class GetUserRequest : IRequest<GetUserResponse>
{
    public Guid UserId { get; set; }
}

// Endpoint sends request
var response = await _mediator.Send(request, cancellationToken);

// Handler processes request
public class GetUserHandler : IRequestHandler<GetUserRequest, GetUserResponse>
{
    public async ValueTask<GetUserResponse> Handle(...)
    {
        // Business logic
    }
}
```

**Benefits**:
- Loose coupling between API and business logic
- Easy to add cross-cutting concerns (logging, validation)
- Testable in isolation

### 2. Repository Pattern

**Purpose**: Abstracts data access logic

**Implementation**:

```csharp
// Interface in Domain layer
public interface IObjectRepository
{
    Task<ObjectProperties?> GetByIdAsync(string id);
    Task AddEventAsync(Guid userId, IEvent @event);
}

// Implementation in Infrastructure layer
public class ObjectRepository : IObjectRepository
{
    private readonly IDocumentSession _session;
    
    public async Task<ObjectProperties?> GetByIdAsync(string id)
    {
        return await _session.LoadAsync<ObjectProperties>(id);
    }
}
```

**Benefits**:
- Domain doesn't depend on database implementation
- Easy to mock for testing
- Can swap database without changing business logic

### 3. Dependency Injection

**Purpose**: Inversion of control for loose coupling

**Implementation**:

```csharp
// Registration (Infrastructure/DependencyInjection.cs)
services.AddScoped<IObjectRepository, ObjectRepository>();
services.AddScoped<IPhotoService, PhotoService>();

// Injection via constructor
public class UploadPhotoHandler
{
    private readonly IObjectRepository _repository;
    private readonly IPhotoService _photoService;
    
    public UploadPhotoHandler(
        IObjectRepository repository,
        IPhotoService photoService)
    {
        _repository = repository;
        _photoService = photoService;
    }
}
```

**Benefits**:
- Testable (can inject mocks)
- Flexible (can swap implementations)
- Follows Dependency Inversion Principle

### 4. Event Sourcing

**Purpose**: Store state changes as a sequence of events

**Implementation**: Using Marten Event Store

```csharp
// Domain Event
public record MediaObjectCreated(
    string ObjectId, 
    string Hash, 
    DateTimeOffset Timestamp);

// Append event
_session.Events.Append(userId, new MediaObjectCreated(objectId, hash, DateTime.UtcNow));
await _session.SaveChangesAsync();

// Query events
var events = await _session.Events.FetchStreamAsync(userId);
```

**Benefits**:
- Complete audit trail
- Can rebuild state from events
- Supports temporal queries

### 5. Soft Delete Pattern

**Purpose**: Mark records as deleted without removing them

**Implementation**: Using Marten's soft delete with partitioning

```csharp
// Configure in Infrastructure
options.Schema.For<ObjectProperties>()
    .SoftDeletedWithPartitioningAndIndex()
    .Metadata(m =>
    {
        m.IsSoftDeleted.MapTo(x => x.Deleted);
        m.SoftDeletedAt.MapTo(x => x.DeletedAt);
    });

// Soft delete
_session.Delete(obj);
await _session.SaveChangesAsync();

// Query soft-deleted items
var trashedItems = await _session.Query<ObjectProperties>()
    .Where(x => x.IsDeleted())
    .ToListAsync();
```

**Benefits**:
- Data recovery
- Audit trail
- Compliance with regulations

---

## Project Structure

### Solution Organization

```
SF.PhotoPixels.sln
├── src/
│   ├── SF.PhotoPixels.API              # Presentation layer
│   ├── SF.PhotoPixels.Application      # Application layer
│   ├── SF.PhotoPixels.Domain           # Domain layer
│   ├── SF.PhotoPixels.Infrastructure   # Infrastructure layer
│   └── libraries/
│       └── SolidTUS/                   # External library
└── tests/
    └── SF.PhotoPixels.API.Integration.Tests
```

### Feature Organization

Within each layer, code is organized **by feature**, not by type:

```
Application/
├── Commands/
│   ├── User/
│   │   ├── CreateUser/
│   │   │   ├── CreateUserRequest.cs
│   │   │   ├── CreateUserHandler.cs
│   │   │   └── CreateUserValidator.cs
│   │   ├── DeleteUser/
│   │   └── AdjustQuota/
│   └── ObjectVersioning/
└── Query/
    ├── Album/
    │   └── GetAlbums/
    │       ├── GetAlbumsRequest.cs
    │       ├── GetAlbumsHandler.cs
    │       └── GetAlbumsResponse.cs
    └── PhotoStorage/
```

**Benefits**:
- Easy to find related code
- Clear feature boundaries
- Easier to add/remove features

---

## Layer Details

### 1. API Layer (`SF.PhotoPixels.API`)

**Responsibilities**:
- HTTP routing and endpoint definitions
- Request/response serialization
- Authentication and authorization
- Input validation (basic)
- Swagger documentation
- Exception handling middleware

**Key Files**:
- `Program.cs`: Application configuration and startup
- `Endpoints/`: API endpoints grouped by feature
- `Middlewares/`: Cross-cutting concerns
- `Security/`: Authorization policies
- `Extensions/`: Service registration

**What it does NOT do**:
- Business logic
- Data access
- External service calls

### 2. Application Layer (`SF.PhotoPixels.Application`)

**Responsibilities**:
- Orchestrate business workflows
- Execute use cases (commands & queries)
- Validate business rules
- Transform domain models to DTOs
- Coordinate between domain and infrastructure

**Key Files**:
- `Commands/`: State-changing operations
- `Query/`: Data retrieval operations
- `Core/`: Shared application services
- `Pipelines/`: Cross-cutting behaviors (validation, logging)
- `Security/`: Authentication services

**What it does NOT do**:
- HTTP concerns
- Direct database access
- Define domain entities

### 3. Domain Layer (`SF.PhotoPixels.Domain`)

**Responsibilities**:
- Define business entities
- Enforce business rules and invariants
- Emit domain events
- Define repository contracts
- Pure business logic

**Key Files**:
- `Entities/`: Core business objects (User, Album, ObjectProperties)
- `Events/`: Domain events
- `Repositories/`: Repository interfaces
- `Enums/`: Domain enumerations
- `Exceptions/`: Domain-specific exceptions

**What it does NOT do**:
- Database implementation
- HTTP handling
- External service calls
- Infrastructure concerns

### 4. Infrastructure Layer (`SF.PhotoPixels.Infrastructure`)

**Responsibilities**:
- Implement repository interfaces
- Data persistence (Marten/PostgreSQL)
- File storage
- External service integrations
- Background services
- Database migrations

**Key Files**:
- `Repositories/`: Repository implementations
- `Storage/`: File storage services
- `Services/`: Image/video processing, email
- `Migrations/`: Database migration scripts
- `BackgroundServices/`: Long-running tasks
- `Stores/`: ASP.NET Identity implementations

**What it does NOT do**:
- Define business rules
- HTTP routing
- Application workflows

---

## Data Flow

### Command Flow Example: Upload Photo

```
1. HTTP Request
   ↓
2. API Endpoint (UploadPhotoEndpoint)
   ↓
3. Mediator.Send(UploadPhotoRequest)
   ↓
4. Application Handler (UploadPhotoHandler)
   ├─→ Validate input
   ├─→ Process image (IPhotoService)
   ├─→ Save to storage (IObjectStorage)
   ├─→ Create domain entity (ObjectProperties)
   ├─→ Save to database (IDocumentSession)
   └─→ Emit event (MediaObjectCreated)
   ↓
5. Infrastructure
   ├─→ Marten persists to PostgreSQL
   ├─→ File written to disk
   └─→ Event stored in event stream
   ↓
6. HTTP Response (UploadPhotoResponse)
```

### Query Flow Example: Get Albums

```
1. HTTP Request
   ↓
2. API Endpoint (GetAlbumsEndpoint)
   ↓
3. Mediator.Send(GetAlbumsRequest)
   ↓
4. Application Handler (GetAlbumsHandler)
   ├─→ Query database (IDocumentSession)
   ├─→ Map to DTOs
   └─→ Return response
   ↓
5. Infrastructure
   └─→ Marten queries PostgreSQL
   ↓
6. HTTP Response (GetAlbumsResponse)
```

---

## Best Practices

### 1. Keep Domain Pure

```csharp
// ✅ Good: Pure domain logic
public class Album
{
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty");
        
        Name = newName;
    }
}

// ❌ Bad: Infrastructure concerns in domain
public class Album
{
    public void Save()
    {
        _dbContext.SaveChanges(); // NO!
    }
}
```

### 2. Use Interfaces for Dependencies

```csharp
// ✅ Good: Depend on abstraction
public class UploadHandler
{
    private readonly IPhotoService _photoService;
}

// ❌ Bad: Depend on concrete implementation
public class UploadHandler
{
    private readonly PhotoService _photoService;
}
```

### 3. Keep Handlers Focused

```csharp
// ✅ Good: Single responsibility
public class CreateAlbumHandler : IRequestHandler<CreateAlbumRequest, CreateAlbumResponse>
{
    public async ValueTask<CreateAlbumResponse> Handle(...)
    {
        // Only handle album creation
    }
}

// ❌ Bad: Multiple responsibilities
public class AlbumHandler
{
    public void Create(...) { }
    public void Update(...) { }
    public void Delete(...) { }
}
```

### 4. Use Value Objects

```csharp
// ✅ Good: Encapsulate related data
public record Address(string Street, string City, string PostalCode);

// ❌ Bad: Primitive obsession
public class User
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
}
```

---

## Further Reading

- [Clean Architecture Book by Robert C. Martin](https://www.amazon.com/Clean-Architecture-Craftsmans-Software-Structure/dp/0134494164)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Domain-Driven Design](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)

---

**Last Updated**: December 2025  
**Version**: 1.0.0

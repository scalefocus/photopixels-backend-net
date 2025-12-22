# PhotoPixels Backend - Developer Guide

Welcome to the PhotoPixels backend developer documentation! This guide serves as the central hub for all development resources.

---

## 📚 Documentation Index

### Quick Start
- **[Getting Started](./GETTING_STARTED.md)** - Setup instructions, prerequisites, and first steps for new developers

### Core Concepts
- **[Architecture](./ARCHITECTURE.md)** - Clean Architecture principles, CQRS pattern, and design patterns
- **[API Development](./API_DEVELOPMENT.md)** - Creating endpoints, request/response patterns, and best practices
- **[Database & Migrations](./DATABASE.md)** - Marten document DB, migrations, querying, and event sourcing

### Development Workflow
- **[Testing](./TESTING.md)** - Integration testing strategies, writing tests, and test helpers
- **[Deployment](./DEPLOYMENT.md)** - Docker builds, CI/CD pipeline, and production deployment

### Reference
- **[Troubleshooting](./TROUBLESHOOTING.md)** - Common issues and solutions
- **[Software Development Specifications](../PhotoPixels_Software_Development_Specifications.md)** - Detailed project requirements

---

## 🎯 Quick Links by Role

### New Developer
1. Start with [Getting Started](./GETTING_STARTED.md) to set up your environment
2. Read [Architecture](./ARCHITECTURE.md) to understand the project structure
3. Follow [API Development](./API_DEVELOPMENT.md) to create your first endpoint

### Backend Developer
1. Review [API Development](./API_DEVELOPMENT.md) for endpoint patterns
2. Study [Database & Migrations](./DATABASE.md) for data access
3. Check [Testing](./TESTING.md) for test-driven development

### DevOps Engineer
1. Read [Deployment](./DEPLOYMENT.md) for CI/CD and production setup
2. Review [Getting Started](./GETTING_STARTED.md) for Docker Compose
3. Consult [Troubleshooting](./TROUBLESHOOTING.md) for production issues

### QA Engineer
1. Start with [Testing](./TESTING.md) for test structure and patterns
2. Review [API Development](./API_DEVELOPMENT.md) to understand endpoints
3. Use [Troubleshooting](./TROUBLESHOOTING.md) for debugging test failures

---

## 📖 What is PhotoPixels?

PhotoPixels is an open-source, self-hosted photo management API built with .NET 8. It provides:

- **Photo & Video Management**: Upload, organize, and manage media files
- **Album Organization**: Create and manage photo albums
- **User Authentication**: Secure JWT-based authentication
- **Resumable Uploads**: TUS protocol for large file uploads
- **Quota Management**: Per-user storage quotas
- **Soft Delete**: Trash functionality with recovery
- **EXIF Metadata**: Automatic extraction of photo metadata

---

## 🛠️ Technology Stack

| Category | Technology |
|----------|------------|
| **Framework** | .NET 8 / ASP.NET Core |
| **Database** | PostgreSQL with Marten (Document DB & Event Store) |
| **Authentication** | JWT with Microsoft.Identity |
| **Image Processing** | ImageSharp, MetadataExtractor |
| **Video Processing** | FFmpeg |
| **Resumable Uploads** | SolidTUS (TUS Protocol) |
| **Email** | MailKit |
| **API Documentation** | Swagger/OpenAPI |
| **Testing** | xUnit, TestContainers, FluentAssertions |
| **Containerization** | Docker, Docker Compose |
| **CI/CD** | GitHub Actions |

---

## 🏗️ Architecture Overview

PhotoPixels follows **Clean Architecture** with **CQRS** (Command Query Responsibility Segregation):

```
┌─────────────────────────────────────────┐
│         SF.PhotoPixels.API              │  ← Presentation Layer (Controllers, Endpoints)
│         (ASP.NET Core)                  │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│      SF.PhotoPixels.Application         │  ← Application Layer (Commands, Queries, Handlers)
│      (Mediator, CQRS)                   │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│       SF.PhotoPixels.Domain             │  ← Domain Layer (Entities, Business Logic)
│       (Entities, Events)                │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│    SF.PhotoPixels.Infrastructure        │  ← Infrastructure Layer (Database, Services)
│    (Marten, Email, File Storage)        │
└─────────────────────────────────────────┘
```

**Learn more**: [Architecture Guide](./ARCHITECTURE.md)

---

## 🚀 Project Structure

```
photopixels-backend-net/
├── src/
│   ├── SF.PhotoPixels.API/              # API endpoints, authentication, middleware
│   ├── SF.PhotoPixels.Application/      # Commands, queries, handlers (CQRS)
│   ├── SF.PhotoPixels.Domain/           # Entities, events, domain logic
│   └── SF.PhotoPixels.Infrastructure/   # Database, migrations, external services
├── tests/
│   └── SF.PhotoPixels.API.Tests/        # Integration tests
├── docs/                                 # Documentation (you are here!)
├── docker-compose.yml                    # Docker Compose for local development
├── Dockerfile                            # Production Docker image
└── README.md                             # Project overview

```

**Organized by feature, not by type**:
- ✅ `Features/Media/Commands/UploadPhoto/`
- ❌ `Controllers/`, `Services/`, `Repositories/`

**Learn more**: [Architecture Guide](./ARCHITECTURE.md)

---

## 📝 Development Guidelines

### Code Style

- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use **meaningful names** for classes, methods, and variables
- Keep methods **small and focused** (single responsibility)
- Prefer **async/await** for I/O operations
- Use **nullable reference types** (`#nullable enable`)

### Best Practices

1. **Commands vs Queries**:
   - Commands: Modify state, return `Result<T>` with OneOf
   - Queries: Read state, return DTOs

2. **Error Handling**:
   - Use OneOf for discriminated unions (`Result<SuccessDto, NotFound, Unauthorized>`)
   - Return appropriate HTTP status codes

3. **Validation**:
   - Use FluentValidation for request validation
   - Validate in handlers, not controllers

4. **Testing**:
   - Write integration tests for all endpoints
   - Use TestContainers for real PostgreSQL
   - Follow Arrange-Act-Assert pattern

**Learn more**: [API Development Guide](./API_DEVELOPMENT.md)

---

## 🔗 Useful Commands

### Local Development

```powershell
# Start PostgreSQL with Docker Compose
docker-compose up -d

# Run API
dotnet run --project src/SF.PhotoPixels.API/SF.PhotoPixels.API.csproj

# Run tests
dotnet test

# Create migration
.\AddMigration.bat 0007.MyMigration
```

### Docker

```powershell
# Build image
docker build -t photopixels-api .

# Run container
docker run -d -p 8080:8080 --env-file .env photopixels-api
```

**Learn more**: [Getting Started](./GETTING_STARTED.md), [Deployment](./DEPLOYMENT.md)

---

## 🐛 Common Issues

| Issue | Solution |
|-------|----------|
| GitVersion error | Initialize Git repo or remove GitVersion package |
| PostgreSQL connection failed | Verify Docker is running, check connection string |
| Migration fails | Review SQL syntax, check migration logs |
| 401 Unauthorized | Check JWT token, verify secret in config |
| 413 Payload Too Large | Increase max request size in Program.cs |

**Full troubleshooting guide**: [Troubleshooting](./TROUBLESHOOTING.md)

---

## 📞 Getting Help

- **Documentation Issues**: Create GitHub issue
- **Development Questions**: Check [Troubleshooting](./TROUBLESHOOTING.md)
- **Bugs**: Create GitHub issue with reproducible steps
- **Feature Requests**: Create GitHub issue with use case

---

## 📄 License

PhotoPixels is licensed under the MIT License. See [LICENSE](../LICENSE) for details.

---

**Last Updated**: December 2024  
**Version**: 1.0.0
              ↓
┌─────────────────────────────────────────┐
│      SF.PhotoPixels.Application         │  ← Application Layer
│  (Commands, Queries, Handlers, DTOs)    │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│        SF.PhotoPixels.Domain            │  ← Domain Layer
│  (Entities, Events, Interfaces)         │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│     SF.PhotoPixels.Infrastructure       │  ← Infrastructure Layer
│  (Persistence, Services, External APIs) │
└─────────────────────────────────────────┘
```

### CQRS Pattern

The application uses **Command Query Responsibility Segregation (CQRS)** with **Mediator** pattern:

- **Commands**: Mutate state (Create, Update, Delete operations)
- **Queries**: Read data without side effects
- **Handlers**: Process commands and queries independently

**Benefits**:
- Clear separation between reads and writes
- Easier to test and maintain
- Better performance optimization opportunities
- Follows Single Responsibility Principle

### Key Design Patterns

- **Mediator Pattern**: Decouples request/response handling using Mediator library
- **Repository Pattern**: Abstracts data access through IObjectRepository, IApplicationConfigurationRepository
- **Dependency Injection**: All dependencies are registered in DI container
- **Event Sourcing**: Uses Marten for event storage (MediaObjectCreated, MediaObjectUpdated, etc.)
- **Soft Delete**: Uses Marten's soft delete with partitioning for ObjectProperties and User entities

---

## Getting Started

### Prerequisites

- **.NET 8 SDK** or later
- **Docker** and **Docker Compose**
- **PostgreSQL 14+** (if running locally without Docker)
- **Git**
- **Visual Studio Code** or **Visual Studio 2022**

### Local Development Setup

#### 1. Clone the Repository

```powershell
git clone https://github.com/your-org/photopixels-backend-net.git
cd photopixels-backend-net
```

#### 2. Environment Configuration

Create a `.env` file in the root directory (copy from `.env.dev` if available):

```env
IMAGE_VERSION=latest
APP_PUBLIC_PORT=5000
DB_PUBLIC_PORT=5432
DB_PASSWORD=your_secure_password
ADMIN_EMAIL=admin@example.com
ADMIN_PASSWORD=Admin123!
PHOTOS_LOCATION=./photos
```

#### 3. Run with Docker Compose

```powershell
docker-compose up
```

The API will be available at:
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **PostgreSQL**: localhost:5432

#### 4. Run Locally (Without Docker)

**Start PostgreSQL** (ensure it's running on port 5432)

**Update `appsettings.Development.json`**:
```json
{
  "ConnectionStrings": {
    "PhotosMetadata": "Host=localhost;Port=5432;Database=photosdb;Username=postgres;Password=your_password"
  }
}
```

**Restore Dependencies**:
```powershell
dotnet restore
```

**Run the Application**:
```powershell
cd src\SF.PhotoPixels.API
dotnet run
```

#### 5. Stop and Reset

To stop containers:
```powershell
docker-compose down
```

To reset database:
```powershell
docker-compose down -v
```

---

## Project Structure

### Solution Overview

```
SF.PhotoPixels.sln
├── src/
│   ├── SF.PhotoPixels.API              # API endpoints, middleware, configuration
│   ├── SF.PhotoPixels.Application      # Business logic, commands, queries
│   ├── SF.PhotoPixels.Domain           # Domain entities, events, interfaces
│   ├── SF.PhotoPixels.Infrastructure   # Data access, external services
│   └── libraries/
│       └── SolidTUS/                   # TUS protocol implementation
├── tests/
│   └── SF.PhotoPixels.API.Integration.Tests
├── docs/                               # Documentation
├── docker/                             # Docker configurations
└── .github/workflows/                  # CI/CD pipelines
```

### Layer Responsibilities

#### 1. **API Layer** (`SF.PhotoPixels.API`)

**Purpose**: HTTP entry point, routing, authentication, error handling

**Key Components**:
- `Endpoints/`: API endpoints organized by feature
  - `PhotosEndpoints/`: Photo upload, download, delete
  - `User/`: User management, authentication
  - `ResumableUploadsEndpoints/`: TUS resumable uploads
  - `Sync/`: Client synchronization
- `Middlewares/`: Exception handling, logging
- `Security/`: Authorization handlers (e.g., RequireAdminRole)
- `Program.cs`: Application startup and configuration

**Responsibilities**:
- Route HTTP requests to appropriate handlers
- Validate request models
- Return HTTP responses
- Handle authentication/authorization
- Swagger documentation

#### 2. **Application Layer** (`SF.PhotoPixels.Application`)

**Purpose**: Orchestrates business logic, use cases, application workflows

**Key Components**:
- `Commands/`: State-changing operations
  - `User/`: CreateUser, DeleteUser, AdjustQuota
  - `ObjectVersioning/`: TrashObject, RestoreObject
  - `Tus/`: CreateUpload, SendChunk
- `Query/`: Read operations
  - `PhotoStorage/`: GetObjectData, GetObjectsList, LoadMedia
  - `Album/`: GetAlbums
- `Core/`: Shared application concerns (ExecutionContext, Validators)
- `Security/`: Authentication logic
- `TrashHardDelete/`: Background service for hard deletes
- `VersionMigrations/`: Application version migrations

**Responsibilities**:
- Execute business logic
- Coordinate between domain and infrastructure
- Handle validation
- Manage transactions
- Transform domain models to DTOs

#### 3. **Domain Layer** (`SF.PhotoPixels.Domain`)

**Purpose**: Core business entities, domain logic, domain events

**Key Components**:
- `Entities/`:
  - `User`: User account with quota management
  - `ObjectProperties`: Photo/video metadata
  - `Album`: Photo album
  - `ObjectAlbum`: Many-to-many relationship between objects and albums
  - `ApplicationConfiguration`: System configuration
- `Events/`:
  - `MediaObjectCreated`
  - `MediaObjectUpdated`
  - `MediaObjectTrashed`
  - `MediaObjectRemovedFromTrash`
  - `MediaObjectDeleted`
- `Repositories/`: Repository interfaces
- `Enums/`: Domain enumerations (Role, MediaType)

**Responsibilities**:
- Define business entities and rules
- Emit domain events
- Enforce invariants
- No dependencies on other layers

#### 4. **Infrastructure Layer** (`SF.PhotoPixels.Infrastructure`)

**Purpose**: External concerns, data persistence, third-party integrations

**Key Components**:
- `Repositories/`: Marten-based repository implementations
- `Storage/`: File storage (LocalObjectStorage, FormattedImage)
- `Services/`:
  - `PhotoService`: Image processing
  - `VideoService`: Video processing
  - `TusService`: Resumable upload handling
- `Migrations/`: Database migration scripts
- `BackgroundServices/`: Long-running tasks
  - `TrashHardDeleteBackgroundService`
  - `ImportDirectoryService`
  - `TusExpirationHandler`
- `Stores/`: ASP.NET Identity user store
- `Projections/`: Marten event projections

**Responsibilities**:
- Persist data to PostgreSQL via Marten
- Store files on disk
- Process images/videos
- Send emails
- Implement cross-cutting concerns

---

## Development Guidelines

### SOLID Principles

The codebase adheres to **SOLID** principles:

1. **Single Responsibility**: Each class has one reason to change
2. **Open/Closed**: Open for extension, closed for modification
3. **Liskov Substitution**: Subtypes must be substitutable for base types
4. **Interface Segregation**: Small, focused interfaces
5. **Dependency Inversion**: Depend on abstractions, not concretions

### Coding Standards

- **Naming Conventions**:
  - PascalCase for classes, methods, properties
  - camelCase for local variables, parameters
  - Prefix interfaces with `I` (e.g., `IObjectRepository`)
- **File Organization**:
  - One class per file
  - File name matches class name
  - Organize by feature, not by type
- **Code Style**:
  - Use `var` where type is obvious
  - Prefer expression-bodied members for simple methods
  - Use `async`/`await` for I/O operations
  - Handle exceptions appropriately (don't swallow)

### Error Handling

- Use `OneOf<TSuccess, TError>` for operation results
- Return typed errors (e.g., `NotFound`, `ValidationError`, `Forbidden`)
- Global exception handling via `ExceptionHandlingMiddleware`
- Log errors with structured logging (Serilog)

**Example**:
```csharp
public async ValueTask<OneOf<GetObjectResponse, NotFound>> Handle(
    GetObjectRequest request, 
    CancellationToken cancellationToken)
{
    var obj = await _session.LoadAsync<ObjectProperties>(request.Id, cancellationToken);
    
    if (obj == null)
        return new NotFound();
    
    return new GetObjectResponse { /* ... */ };
}
```

### Dependency Injection

All services are registered in `DependencyInjection.cs` files per layer:

- `SF.PhotoPixels.API/Extensions/DependencyInjection.cs`
- `SF.PhotoPixels.Application/DependencyInjection.cs`
- `SF.PhotoPixels.Infrastructure/DependencyInjection.cs`

**Example**:
```csharp
public static IServiceCollection AddApplicationServices(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    services.AddMediator(options => 
    { 
        options.ServiceLifetime = ServiceLifetime.Scoped; 
    });
    
    services.AddScoped<ITrashHardDeleteService, TrashHardDeleteService>();
    
    return services;
}
```

---

## API Development

### Creating a New Endpoint

#### 1. Define Request & Response Models

**Application Layer** (`SF.PhotoPixels.Application/Query/Album/GetAlbums.cs`):

```csharp
public class GetAlbumsRequest : IRequest<GetAlbumsResponse>
{
    [FromQuery]
    public int Page { get; set; } = 1;
    
    [FromQuery]
    public int PageSize { get; set; } = 20;
}

public class GetAlbumsResponse
{
    public List<AlbumDto> Albums { get; set; }
    public int TotalCount { get; set; }
}

public class AlbumDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}
```

#### 2. Implement Handler

```csharp
public class GetAlbumsHandler : IQueryHandler<GetAlbumsRequest, GetAlbumsResponse>
{
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    
    public async ValueTask<GetAlbumsResponse> Handle(
        GetAlbumsRequest request, 
        CancellationToken cancellationToken)
    {
        var albums = await _session.Query<Album>()
            .Where(x => x.UserId == _executionContextAccessor.UserId)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        return new GetAlbumsResponse
        {
            Albums = albums.Select(a => new AlbumDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                CreatedDate = a.CreatedDate
            }).ToList(),
            TotalCount = albums.Count
        };
    }
}
```

#### 3. Create Endpoint

**API Layer** (`SF.PhotoPixels.API/Endpoints/Album/GetAlbumsEndpoint.cs`):

```csharp
public class GetAlbumsEndpoint : EndpointBaseAsync
    .WithRequest<GetAlbumsRequest>
    .WithActionResult<GetAlbumsResponse>
{
    private readonly IMediator _mediator;

    public GetAlbumsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/albums")]
    [SwaggerOperation(
        Summary = "Get user's albums",
        Description = "Retrieves paginated list of albums for authenticated user",
        OperationId = "GetAlbums",
        Tags = new[] { "Album operations" }
    )]
    public override async Task<ActionResult<GetAlbumsResponse>> HandleAsync(
        [FromQuery] GetAlbumsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}
```

### Route Parameters

#### From Route
```csharp
[HttpGet("/album/{albumId}")]
public async Task<ActionResult> GetAlbum(
    [FromRoute] Guid albumId,
    CancellationToken cancellationToken)
{
    // ...
}
```

#### From Query
```csharp
[HttpGet("/albums")]
public async Task<ActionResult> GetAlbums(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    // Called as: /albums?page=2&pageSize=50
}
```

#### Mixed Parameters
```csharp
public class GetAlbumItemsRequest
{
    [FromRoute]
    public Guid AlbumId { get; set; }
    
    [FromQuery]
    public int Page { get; set; } = 1;
}

[HttpGet("/album/{albumId}/items")]
public async Task<ActionResult> GetAlbumItems(
    [FromQuery] GetAlbumItemsRequest request)
{
    // Called as: /album/123/items?page=2
}
```

### Authentication & Authorization

All endpoints require authentication by default (configured in `Program.cs`):

```csharp
builder.Services.AddAuthorization(options =>
{
    var lockedDown = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.FallbackPolicy = lockedDown;
});
```

#### Admin-Only Endpoints

```csharp
[HttpPost("/admin/quota")]
[RequireAdminRole]
public async Task<ActionResult> AdjustQuota(
    [FromBody] AdjustQuotaRequest request)
{
    // Only users with Role.Admin can access
}
```

#### Allow Anonymous

```csharp
[HttpPost("/auth/login")]
[AllowAnonymous]
public async Task<ActionResult> Login(
    [FromBody] LoginRequest request)
{
    // Public endpoint
}
```

---

## Database & Migrations

### Marten Document DB

PhotoPixels uses **Marten** as a .NET Document Database and Event Store on top of PostgreSQL.

**Key Features**:
- Document storage (JSON in PostgreSQL)
- Event sourcing
- Soft deletes with partitioning
- LINQ querying
- Indexing and foreign keys

### Schema Configuration

Configured in `SF.PhotoPixels.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddMarten(options =>
{
    options.Connection(connectionString);
    options.DatabaseSchemaName = "photos";
    
    // ObjectProperties with soft delete
    options.Schema.For<ObjectProperties>()
        .SoftDeletedWithPartitioningAndIndex()
        .Index(x => x.Hash)
        .Duplicate(x => x.UserId, configure: idx => idx.IsUnique = false)
        .Metadata(m =>
        {
            m.IsSoftDeleted.MapTo(x => x.Deleted);
            m.SoftDeletedAt.MapTo(x => x.DeletedAt);
        });
    
    // Album with foreign keys
    options.Schema.For<ObjectAlbum>()
        .Duplicate(x => x.AlbumId)
        .Duplicate(x => x.ObjectId)
        .ForeignKey<Album>(x => x.AlbumId)
        .ForeignKey<ObjectProperties>(x => x.ObjectId);
});
```

### Creating Migrations

PhotoPixels uses **DbUp** for SQL migrations.

#### 1. Create Migration Script

Run from repository root:

```powershell
./AddMigration.bat 0006.AddMyNewTable
```

This creates:
- `src/SF.PhotoPixels.Infrastructure/Migrations/0006.AddMyNewTable.sql`
- `src/SF.PhotoPixels.Infrastructure/Migrations/Rollback/0006.AddMyNewTable.sql`

#### 2. Write SQL Migration

```sql
-- 0006.AddMyNewTable.sql
DROP TABLE IF EXISTS photos.mt_doc_mynewentity CASCADE;

CREATE TABLE photos.mt_doc_mynewentity (
    id                  uuid                        NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
    user_id             uuid                        NULL,
    CONSTRAINT pkey_mt_doc_mynewentity_id PRIMARY KEY (id)
);

CREATE INDEX mt_doc_mynewentity_idx_user_id 
    ON photos.mt_doc_mynewentity USING btree (user_id);
```

#### 3. Migrations Run Automatically

Migrations execute on application startup via DbUp in `DependencyInjection.cs`:

```csharp
var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString, "photos")
    .WithScriptsEmbeddedInAssembly(typeof(DependencyInjection).Assembly, 
        s => !s.Contains("drop") && s.EndsWith(".sql"))
    .WithScriptNameComparer(new EmbeddedMigrationScriptComparer())
    .LogToAutodetectedLog()
    .Build();

upgrader.PerformUpgrade();
```

### Important Notes

- **Schema**: Always use `photos` schema
- **Naming**: Marten tables are prefixed with `mt_doc_`
- **Soft Deletes**: Tables with soft delete use composite primary key `(id, mt_deleted)`
- **Foreign Keys**: Cannot directly reference soft-deleted tables; use unique indexes on `id` where `mt_deleted = false`

### Querying with Marten

```csharp
// Simple query
var user = await _session.LoadAsync<User>(userId);

// LINQ query
var albums = await _session.Query<Album>()
    .Where(x => x.UserId == userId && x.Name.Contains("vacation"))
    .OrderByDescending(x => x.CreatedDate)
    .ToListAsync();

// Soft-deleted items
var trashedObjects = await _session.Query<ObjectProperties>()
    .Where(x => x.IsDeleted() && x.UserId == userId)
    .ToListAsync();

// Insert
_session.Insert(newAlbum);
await _session.SaveChangesAsync();

// Update
album.Name = "Updated Name";
_session.Update(album);
await _session.SaveChangesAsync();

// Soft delete
_session.Delete(obj);
await _session.SaveChangesAsync();

// Hard delete
_session.HardDelete(obj);
await _session.SaveChangesAsync();
```

### Event Sourcing

Events are stored in `mt_events` and `mt_streams` tables:

```csharp
// Append event
_session.Events.Append(userId, new MediaObjectCreated(objectId, metadata));
await _session.SaveChangesAsync();

// Query events
var events = await _session.Events.FetchStreamAsync(userId);
```

---

## Testing

### Integration Tests

Located in `SF.PhotoPixels.API.Integration.Tests`.

**Why Integration Tests?**
- Tests real workflows (upload → process → store → retrieve)
- Validates database operations
- Ensures authentication works end-to-end
- More valuable than unit tests for file handling

### Running Tests

```powershell
# All tests
dotnet test

# Exclude integration tests
dotnet test --filter "Category!=Integration"

# Specific test class
dotnet test --filter "FullyQualifiedName~AdjustQuotaEndpointTests"
```

### Test Structure

```csharp
public class AdjustQuotaEndpointTests : IntegrationTest
{
    public AdjustQuotaEndpointTests(PhotosWebApplicationFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task AdjustQuota_WithValidData_ShouldReturnOK()
    {
        // Arrange
        var token = await AuthenticateAsSeededAdminAsync();
        var requestBody = new AdjustQuotaRequest
        {
            Id = Guid.Parse(token.UserId),
            Quota = 100
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quotaResponse = await response.Content.ReadFromJsonAsync<AdjustQuotaResponse>();
        quotaResponse.Quota.Should().Be(100);
    }
}
```

### Test Helpers

**IntegrationTest Base Class**:
- Sets up in-memory test server
- Provides authenticated HTTP client
- Handles test database

**PhotosWebApplicationFactory**:
- Custom WebApplicationFactory
- Configures test environment
- Seeds test data

---

## Deployment

### Docker Build

Build multi-platform image:

```powershell
docker buildx build `
    --platform linux/amd64,linux/arm64,linux/arm/v7 `
    --build-arg VERSION="1.0.0" `
    --build-arg VERSION_SUFFIX="alpha" `
    -t scalefocusad/photopixels-backend-net:1.0.0-alpha `
    -f ./src/SF.PhotoPixels.API/Dockerfile . `
    --push
```

### CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/build_to_publish.yml`):

1. **Checkout**: Fetch code with full Git history
2. **GitVersion**: Calculate semantic version
3. **Restore**: `dotnet restore`
4. **Build**: `dotnet build`
5. **Test**: `dotnet test` (excludes integration tests)
6. **Docker Build**: Multi-platform image build
7. **Docker Push**: Push to Docker Hub

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__PhotosMetadata` | PostgreSQL connection string | `Host=pgsql;Port=5432;Database=photosdb;Username=postgres;Password=secret` |
| `Admin__Email` | Default admin email | `admin@example.com` |
| `Admin__Password` | Default admin password | `Admin123!` |
| `Telemetry__Enabled` | Enable OpenTelemetry | `true` |
| `PHOTOS_LOCATION` | File storage path | `/var/data/sf-photos` |

### Production Checklist

- [ ] Set strong `DB_PASSWORD`
- [ ] Configure `Admin__Email` and `Admin__Password`
- [ ] Set `PHOTOS_LOCATION` to persistent volume
- [ ] Enable HTTPS (configure reverse proxy)
- [ ] Configure SMTP for emails
- [ ] Set up backups for PostgreSQL and photo storage
- [ ] Enable telemetry and monitoring
- [ ] Review CORS settings
- [ ] Configure rate limiting

---

## Troubleshooting

### Common Issues

#### 1. **VS Code Class Navigation Not Working**

**Symptoms**: "Go to Definition" and class outline not working

**Solutions**:
- Install **C# Dev Kit** extension
- Run `dotnet restore`
- Reload VS Code window (`Ctrl+Shift+P` → "Reload Window")
- Check OmniSharp logs (View → Output → OmniSharp Log)
- Ensure `.csproj` and `.sln` files are valid

#### 2. **GitVersion Error: "Not Implemented"**

**Symptoms**: GitVersion fails with `System.NotImplementedException`

**Solutions**:
- Downgrade GitVersion: `dotnet tool update GitVersion.Tool --version 5.12.0 --global`
- Remove `strategies: - Mainline` from `GitVersion.yml` if not needed

#### 3. **Foreign Key Constraint Failed**

**Symptoms**: Cannot create foreign key to soft-deleted table

**Cause**: Marten soft delete uses composite primary key `(id, mt_deleted)`

**Solutions**:
- Create unique index on `id` where `mt_deleted = false`:
  ```sql
  CREATE UNIQUE INDEX mt_doc_objectproperties_unique_id 
      ON photos.mt_doc_objectproperties(id) 
      WHERE mt_deleted = false;
  ```
- Reference the unique index instead of primary key
- Consider managing relationships in application code

#### 4. **Hash Mismatch Between Upload and Storage**

**Cause**: Stream position not reset before hashing

**Solution**:
```csharp
stream.Seek(0, SeekOrigin.Begin);
var hash = await ComputeHashAsync(stream);
stream.Seek(0, SeekOrigin.Begin); // Reset for further use
```

#### 5. **EXIF Metadata Missing**

**Cause**: Image has no EXIF data or wrong format

**Solutions**:
- Fallback to file creation date
- Check if image format supports EXIF (JPEG, TIFF)
- Use alternative EXIF tags (DateTime, DateTimeDigitized)

### Logging

Structured logging with Serilog:

```csharp
_logger.LogInformation("Processing upload for user {UserId}", userId);
_logger.LogWarning("Quota exceeded for user {UserId}", userId);
_logger.LogError(ex, "Failed to process image {ObjectId}", objectId);
```

Logs are written to:
- Console (development)
- Files (production)
- OpenTelemetry (if enabled)

### Monitoring

With telemetry enabled (`Telemetry__Enabled=true`):

- **Prometheus**: Metrics at `/metrics`
- **Grafana**: Dashboards at http://localhost:3000
- **Tempo**: Distributed tracing
- **Health Checks**: `/health`

---

## Additional Resources

### Documentation
- [Clean Architecture](https://positiwise.com/blog/clean-architecture-net-core)
- [CQRS with MediatR](https://medium.com/@1nderj1t/implement-cqrs-design-pattern-with-mediatr-in-asp-net-core-6-c-dc192811694e)
- [Marten Documentation](https://martendb.io/)
- [TUS Protocol](https://tus.io/)
- [Migrations Guide](./Migrations.md)

### Internal Documentation
- [Software Development Specifications](../PhotoPixels_Software_Development_Specifications.md)
- [README](../README.md)

### Support
- GitHub Issues: [Report a bug](https://github.com/your-org/photopixels-backend-net/issues)
- Discussions: [Ask a question](https://github.com/your-org/photopixels-backend-net/discussions)

---

**Last Updated**: December 2025  
**Version**: 1.0.0

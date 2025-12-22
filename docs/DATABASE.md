# Database & Migrations Guide

This guide covers database management, Marten document database configuration, migrations, querying patterns, event sourcing, and soft deletes in the PhotoPixels backend.

---

## Table of Contents

1. [Overview](#overview)
2. [Marten Document Database](#marten-document-database)
3. [Schema Configuration](#schema-configuration)
4. [Migrations](#migrations)
5. [Querying Patterns](#querying-patterns)
6. [Event Sourcing](#event-sourcing)
7. [Soft Deletes](#soft-deletes)
8. [Best Practices](#best-practices)
9. [Common Patterns](#common-patterns)

---

## Overview

### Technology Stack

- **Database**: PostgreSQL 14+
- **ORM**: Marten (Document Database & Event Store)
- **Migrations**: DbUp (SQL-based migrations)
- **Schema**: `photos` (all tables in this schema)

### Why Marten?

Marten provides:
- **Document Storage**: Store .NET objects as JSON in PostgreSQL
- **Event Sourcing**: Built-in event store for domain events
- **LINQ Support**: Query documents using LINQ
- **Soft Deletes**: Partitioning support for soft-deleted records
- **Performance**: Leverages PostgreSQL's JSON capabilities
- **No ORM Impedance**: Direct object-to-JSON mapping

---

## Marten Document Database

### What is Marten?

Marten is a .NET library that turns PostgreSQL into a document database and event store. It stores .NET objects as JSONB documents while providing:

- Full ACID transactions
- Rich querying via LINQ
- Indexing and foreign keys
- Event sourcing capabilities
- Soft delete with partitioning

### Connection Configuration

Located in `SF.PhotoPixels.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddMarten(options =>
{
    // Connection string from configuration
    options.Connection(connectionString);
    
    // Schema name (all tables will be in 'photos' schema)
    options.DatabaseSchemaName = "photos";
    
    // Document mappings (see next section)
    options.Schema.For<ObjectProperties>()
        .SoftDeletedWithPartitioningAndIndex()
        .Index(x => x.Hash)
        .Duplicate(x => x.UserId);
    
    // Event store configuration
    options.Events.StreamIdentity = StreamIdentity.AsGuid;
});
```

### Document Session

Marten uses `IDocumentSession` for database operations (similar to Entity Framework's `DbContext`):

```csharp
public class GetObjectHandler
{
    private readonly IDocumentSession _session;

    public GetObjectHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<ObjectProperties?> GetObject(Guid id)
    {
        // Load document by ID
        return await _session.LoadAsync<ObjectProperties>(id);
    }
}
```

**Session Lifecycle**:
- Scoped per HTTP request
- Auto-injected via DI
- Changes tracked automatically
- Call `SaveChangesAsync()` to persist

---

## Schema Configuration

### Entity Mapping

Each domain entity is mapped to a Marten document table:

#### ObjectProperties (Photos/Videos)

```csharp
options.Schema.For<ObjectProperties>()
    .SoftDeletedWithPartitioningAndIndex()  // Enable soft delete
    .Index(x => x.Hash)                     // Index on file hash
    .Duplicate(x => x.UserId, configure: idx => idx.IsUnique = false)
    .Metadata(m =>
    {
        m.IsSoftDeleted.MapTo(x => x.Deleted);
        m.SoftDeletedAt.MapTo(x => x.DeletedAt);
    });
```

**Key Features**:
- Soft delete with partitioning (composite PK: `id`, `mt_deleted`)
- Indexed hash for duplicate detection
- User ID duplicated to column for efficient queries
- Maps Marten metadata to domain properties

#### Album

```csharp
options.Schema.For<Album>()
    .Duplicate(x => x.UserId, configure: idx => idx.IsUnique = false);
```

**Key Features**:
- Standard document storage
- User ID indexed for filtering

#### ObjectAlbum (Many-to-Many Join)

```csharp
options.Schema.For<ObjectAlbum>()
    .Duplicate(x => x.AlbumId)
    .Duplicate(x => x.ObjectId)
    .ForeignKey<Album>(x => x.AlbumId)
    .ForeignKey<ObjectProperties>(x => x.ObjectId);
```

**Key Features**:
- Join table for Album ↔ ObjectProperties
- Foreign keys to both entities
- Indexed on both IDs

#### User

```csharp
options.Schema.For<User>()
    .SoftDeletedWithPartitioningAndIndex()
    .UniqueIndex(UniqueIndexType.Computed, x => x.Email)
    .Metadata(m =>
    {
        m.IsSoftDeleted.MapTo(x => x.Deleted);
        m.SoftDeletedAt.MapTo(x => x.DeletedAt);
    });
```

**Key Features**:
- Soft delete enabled
- Unique email constraint
- Stores user authentication and quota data

#### ApplicationConfiguration

```csharp
options.Schema.For<ApplicationConfiguration>()
    .Identity(x => x.Id)
    .DatabaseSchemaName("photos");
```

**Key Features**:
- Singleton configuration document
- Custom ID field

### Table Naming Convention

Marten auto-generates table names:

| Entity | Table Name |
|--------|------------|
| `ObjectProperties` | `photos.mt_doc_objectproperties` |
| `Album` | `photos.mt_doc_album` |
| `ObjectAlbum` | `photos.mt_doc_objectalbum` |
| `User` | `photos.mt_doc_user` |
| `ApplicationConfiguration` | `photos.mt_doc_applicationconfiguration` |

**Events**: `photos.mt_events`, `photos.mt_streams`

---

## Migrations

### Migration System

PhotoPixels uses **DbUp** for SQL-based migrations:

- Embedded SQL scripts in `SF.PhotoPixels.Infrastructure/Migrations/`
- Automatic execution on startup
- Version tracking in `photos.schemaversions` table
- Rollback scripts in `Migrations/Rollback/`

### Creating a Migration

#### Step 1: Generate Migration Files

Run from repository root:

```powershell
.\AddMigration.bat 0007.AddNewFeature
```

This creates:
- `src/SF.PhotoPixels.Infrastructure/Migrations/0007.AddNewFeature.sql`
- `src/SF.PhotoPixels.Infrastructure/Migrations/Rollback/0007.AddNewFeature.sql`

#### Step 2: Write Migration SQL

**Forward Migration** (`0007.AddNewFeature.sql`):

```sql
-- Drop existing table if it exists
DROP TABLE IF EXISTS photos.mt_doc_myentity CASCADE;

-- Create new document table
CREATE TABLE photos.mt_doc_myentity (
    id                  uuid                        NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
    user_id             uuid                        NULL,
    CONSTRAINT pkey_mt_doc_myentity_id PRIMARY KEY (id)
);

-- Create indexes
CREATE INDEX mt_doc_myentity_idx_user_id 
    ON photos.mt_doc_myentity USING btree (user_id);

-- Add foreign key (if needed)
ALTER TABLE photos.mt_doc_myentity 
    ADD CONSTRAINT fk_myentity_user 
    FOREIGN KEY (user_id) 
    REFERENCES photos.mt_doc_user(id) 
    ON DELETE CASCADE;
```

**Rollback Migration** (`Rollback/0007.AddNewFeature.sql`):

```sql
-- Rollback: Drop the table
DROP TABLE IF EXISTS photos.mt_doc_myentity CASCADE;
```

#### Step 3: Set Build Action

Ensure migration files are **Embedded Resources** in `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Migrations\*.sql" />
</ItemGroup>
```

#### Step 4: Run Migrations

Migrations run automatically on app startup via `DependencyInjection.cs`:

```csharp
var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString, "photos")
    .WithScriptsEmbeddedInAssembly(
        typeof(DependencyInjection).Assembly, 
        s => !s.Contains("drop") && s.EndsWith(".sql"))
    .WithScriptNameComparer(new EmbeddedMigrationScriptComparer())
    .LogToAutodetectedLog()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    throw result.Error;
}
```

### Migration Naming Convention

Format: `XXXX.DescriptiveName.sql`

Examples:
- `0001.InitialSchema.sql`
- `0002.AddAlbums.sql`
- `0003.AddSoftDeleteToUser.sql`
- `0004.AddUniqueIndexForObjectId.sql`

**Important**: Migrations execute in alphanumeric order based on filename.

### Common Migration Patterns

#### Adding a Column to Existing Table

```sql
-- Add new column to JSONB data
-- Marten stores entity properties in the 'data' JSONB column
-- No schema change needed - just update your C# entity

-- If you need an indexed column:
ALTER TABLE photos.mt_doc_objectproperties 
    ADD COLUMN created_date timestamp with time zone;

CREATE INDEX idx_objectproperties_created_date 
    ON photos.mt_doc_objectproperties(created_date);
```

#### Creating Unique Index (Workaround for Soft Delete FK)

```sql
-- Soft-deleted tables have composite PK (id, mt_deleted)
-- Foreign keys can't reference them directly
-- Solution: Create unique index on id WHERE mt_deleted = false

CREATE UNIQUE INDEX mt_doc_objectproperties_unique_id 
    ON photos.mt_doc_objectproperties(id) 
    WHERE mt_deleted = false;

-- Now other tables can reference this unique index
ALTER TABLE photos.mt_doc_objectalbum 
    ADD CONSTRAINT fk_objectalbum_object 
    FOREIGN KEY (object_id) 
    REFERENCES photos.mt_doc_objectproperties(id);
```

#### Adding Event Projection

```sql
-- No migration needed for event projections
-- Marten automatically creates mt_events and mt_streams tables
-- Events are stored as JSONB in mt_events
```

---

## Querying Patterns

### Basic CRUD Operations

#### Load by ID

```csharp
var obj = await _session.LoadAsync<ObjectProperties>(objectId);

if (obj == null)
{
    return new NotFound();
}
```

#### Query with LINQ

```csharp
var albums = await _session.Query<Album>()
    .Where(x => x.UserId == userId)
    .Where(x => x.Name.Contains(searchTerm))
    .OrderByDescending(x => x.CreatedDate)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

#### Insert

```csharp
var newAlbum = new Album
{
    Id = Guid.NewGuid(),
    UserId = userId,
    Name = "Vacation 2024",
    CreatedDate = DateTimeOffset.UtcNow
};

_session.Insert(newAlbum);
await _session.SaveChangesAsync();
```

#### Update

```csharp
var album = await _session.LoadAsync<Album>(albumId);

album.Name = "Updated Name";
album.Description = "New description";

_session.Update(album);
await _session.SaveChangesAsync();
```

#### Delete (Soft)

```csharp
_session.Delete(obj);
await _session.SaveChangesAsync();

// Object is marked as deleted (mt_deleted = true)
// Still exists in database for recovery
```

#### Hard Delete

```csharp
_session.HardDelete(obj);
await _session.SaveChangesAsync();

// Object is permanently removed from database
```

### Advanced Queries

#### Counting

```csharp
var count = await _session.Query<ObjectProperties>()
    .Where(x => x.UserId == userId && !x.IsDeleted())
    .CountAsync();
```

#### Existence Check

```csharp
var exists = await _session.Query<ObjectProperties>()
    .AnyAsync(x => x.Hash == fileHash && x.UserId == userId);
```

#### Aggregation

```csharp
var totalSize = await _session.Query<ObjectProperties>()
    .Where(x => x.UserId == userId && !x.IsDeleted())
    .SumAsync(x => x.Size);
```

#### Batch Operations

```csharp
var objectsToDelete = await _session.Query<ObjectProperties>()
    .Where(x => x.UserId == userId && x.DeletedAt < cutoffDate)
    .ToListAsync();

foreach (var obj in objectsToDelete)
{
    _session.HardDelete(obj);
}

await _session.SaveChangesAsync();
```

#### Raw SQL

```csharp
var results = await _session.QueryAsync<ObjectProperties>(
    "SELECT data FROM photos.mt_doc_objectproperties WHERE user_id = ?",
    userId);
```

---

## Event Sourcing

### Event Store

Marten provides built-in event sourcing via `mt_events` and `mt_streams` tables.

**Events Table Structure**:
- `id`: Event ID (UUID)
- `stream_id`: Aggregate ID (e.g., User ID)
- `version`: Event version in stream
- `data`: Event payload (JSONB)
- `type`: Event type (e.g., `media_object_created`)
- `timestamp`: When event occurred

### Domain Events

Located in `SF.PhotoPixels.Domain/Events/`:

```csharp
public record MediaObjectCreated(
    Guid ObjectId,
    string Hash,
    long Size,
    DateTimeOffset CreatedDate
);

public record MediaObjectTrashed(
    Guid ObjectId,
    DateTimeOffset TrashedAt
);

public record MediaObjectDeleted(
    Guid ObjectId,
    DateTimeOffset DeletedAt
);
```

### Appending Events

```csharp
public class TrashObjectHandler
{
    private readonly IDocumentSession _session;

    public async Task Handle(TrashObjectRequest request)
    {
        var obj = await _session.LoadAsync<ObjectProperties>(request.ObjectId);
        
        // Soft delete the object
        _session.Delete(obj);
        
        // Append event to stream
        _session.Events.Append(
            obj.UserId,  // Stream ID (user's event stream)
            new MediaObjectTrashed(obj.Id, DateTimeOffset.UtcNow)
        );
        
        await _session.SaveChangesAsync();
    }
}
```

### Querying Events

```csharp
// Fetch entire event stream for a user
var events = await _session.Events.FetchStreamAsync(userId);

foreach (var evt in events)
{
    Console.WriteLine($"{evt.Timestamp}: {evt.EventType} - {evt.Data}");
}

// Fetch specific event types
var trashedEvents = events
    .Where(e => e.Data is MediaObjectTrashed)
    .Select(e => (MediaObjectTrashed)e.Data);
```

### Event Projections

Projections transform events into read models.

**Example**: Track user's total storage used

```csharp
public class UserStorageProjection : IProjection
{
    public void Apply(
        IDocumentSession session,
        EventStream[] streams)
    {
        foreach (var stream in streams)
        {
            var userId = stream.Id;
            var totalSize = 0L;

            foreach (var evt in stream.Events)
            {
                switch (evt.Data)
                {
                    case MediaObjectCreated created:
                        totalSize += created.Size;
                        break;
                    case MediaObjectDeleted deleted:
                        totalSize -= deleted.Size;
                        break;
                }
            }

            // Update user's quota tracking
            var user = session.Load<User>(userId);
            user.UsedQuota = totalSize;
            session.Update(user);
        }
    }
}
```

---

## Soft Deletes

### What is Soft Delete?

Soft delete marks records as deleted without physically removing them. This enables:
- **Trash/Restore** functionality
- **Audit trails**
- **Data recovery**
- **Regulatory compliance**

### Marten Soft Delete with Partitioning

Marten's soft delete uses **partitioning** to maintain performance:

- **Composite Primary Key**: `(id, mt_deleted)`
- **Partition on mt_deleted**: Active (`false`) and deleted (`true`) records in separate partitions
- **Efficient Queries**: Queries on active records are fast (partition pruning)

### Enabling Soft Delete

```csharp
options.Schema.For<ObjectProperties>()
    .SoftDeletedWithPartitioningAndIndex()
    .Metadata(m =>
    {
        m.IsSoftDeleted.MapTo(x => x.Deleted);
        m.SoftDeletedAt.MapTo(x => x.DeletedAt);
    });
```

**Domain Entity**:

```csharp
public class ObjectProperties
{
    public Guid Id { get; set; }
    // ...other properties...
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
```

### Soft Delete Operations

#### Soft Delete

```csharp
_session.Delete(obj);
await _session.SaveChangesAsync();

// Sets: Deleted = true, DeletedAt = current timestamp
```

#### Query Active Records

```csharp
// Automatically excludes soft-deleted records
var activeObjects = await _session.Query<ObjectProperties>()
    .Where(x => x.UserId == userId)
    .ToListAsync();
```

#### Query Deleted Records

```csharp
var trashedObjects = await _session.Query<ObjectProperties>()
    .Where(x => x.IsDeleted() && x.UserId == userId)
    .ToListAsync();
```

#### Restore (Undelete)

Marten doesn't have built-in "undelete". Workaround:

```csharp
// Load deleted object (requires special handling)
var obj = await _session.Query<ObjectProperties>()
    .Where(x => x.Id == objectId && x.IsDeleted())
    .FirstOrDefaultAsync();

// Hard delete the soft-deleted record
_session.HardDelete(obj);

// Re-insert with Deleted = false
obj.Deleted = false;
obj.DeletedAt = null;
_session.Insert(obj);

await _session.SaveChangesAsync();
```

#### Hard Delete (Permanent)

```csharp
_session.HardDelete(obj);
await _session.SaveChangesAsync();

// Record is permanently removed
```

### Foreign Key Challenges

**Problem**: Cannot create foreign key to soft-deleted table (composite PK)

**Solution**: Create unique index on `id` where `mt_deleted = false`

```sql
CREATE UNIQUE INDEX mt_doc_objectproperties_unique_id 
    ON photos.mt_doc_objectproperties(id) 
    WHERE mt_deleted = false;

-- Now can reference this unique index
ALTER TABLE photos.mt_doc_objectalbum 
    ADD CONSTRAINT fk_objectalbum_object 
    FOREIGN KEY (object_id) 
    REFERENCES photos.mt_doc_objectproperties(id);
```

---

## Best Practices

### Transaction Management

```csharp
// All operations within a request share the same session
// SaveChangesAsync() commits transaction

public async Task Handle(CreateAlbumRequest request)
{
    var album = new Album { /* ... */ };
    _session.Insert(album);
    
    var relation = new ObjectAlbum { AlbumId = album.Id, /* ... */ };
    _session.Insert(relation);
    
    // Both inserts committed atomically
    await _session.SaveChangesAsync();
}
```

### Optimistic Concurrency

Marten uses `mt_version` for optimistic concurrency:

```csharp
try
{
    album.Name = "Updated";
    _session.Update(album);
    await _session.SaveChangesAsync();
}
catch (ConcurrencyException)
{
    // Another process modified this document
    // Reload and retry
}
```

### Performance Tips

1. **Use indexes on frequently queried fields**:
   ```csharp
   .Index(x => x.Hash)
   .Duplicate(x => x.UserId)
   ```

2. **Batch operations**:
   ```csharp
   foreach (var item in items)
   {
       _session.Insert(item);
   }
   await _session.SaveChangesAsync(); // Single transaction
   ```

3. **Project only needed fields**:
   ```csharp
   var ids = await _session.Query<ObjectProperties>()
       .Select(x => x.Id)
       .ToListAsync();
   ```

4. **Use CountAsync() instead of ToListAsync().Count()**:
   ```csharp
   var count = await _session.Query<Album>().CountAsync(); // Efficient
   ```

### Schema Design

1. **Duplicate frequently queried fields**: Use `.Duplicate(x => x.PropertyName)` to extract JSONB properties to indexed columns
2. **Avoid over-normalization**: Embrace denormalization where appropriate (document DB strength)
3. **Use events for audit trail**: Append events for significant state changes
4. **Plan for soft delete**: Decide upfront which entities need soft delete

---

## Common Patterns

### Pagination

```csharp
public async Task<PagedResult<Album>> GetAlbums(int page, int pageSize)
{
    var query = _session.Query<Album>()
        .Where(x => x.UserId == _userId);
    
    var totalCount = await query.CountAsync();
    
    var items = await query
        .OrderByDescending(x => x.CreatedDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<Album>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### Duplicate Detection

```csharp
public async Task<bool> IsDuplicate(string hash)
{
    return await _session.Query<ObjectProperties>()
        .AnyAsync(x => x.Hash == hash && x.UserId == _userId && !x.IsDeleted());
}
```

### User Quota Tracking

```csharp
public async Task<long> GetUsedQuota(Guid userId)
{
    return await _session.Query<ObjectProperties>()
        .Where(x => x.UserId == userId && !x.IsDeleted())
        .SumAsync(x => x.Size);
}
```

### Trash Management

```csharp
public async Task EmptyTrash(Guid userId)
{
    var trashedObjects = await _session.Query<ObjectProperties>()
        .Where(x => x.UserId == userId && x.IsDeleted())
        .ToListAsync();
    
    foreach (var obj in trashedObjects)
    {
        _session.HardDelete(obj);
    }
    
    await _session.SaveChangesAsync();
}
```

### Bulk Import

```csharp
public async Task ImportPhotos(List<ObjectProperties> objects)
{
    // Batch insert
    _session.InsertObjects(objects);
    
    // Batch events
    foreach (var obj in objects)
    {
        _session.Events.Append(obj.UserId, new MediaObjectCreated(/* ... */));
    }
    
    await _session.SaveChangesAsync();
}
```

---

## Additional Resources

- [Marten Documentation](https://martendb.io/)
- [DbUp Documentation](https://dbup.readthedocs.io/)
- [PostgreSQL JSONB](https://www.postgresql.org/docs/current/datatype-json.html)
- [Event Sourcing Patterns](https://martinfowler.com/eaaDev/EventSourcing.html)

---

**Next Steps**:
- Review [API Development Guide](./API_DEVELOPMENT.md) for handling database operations in endpoints
- See [Architecture Guide](./ARCHITECTURE.md) for how database fits into Clean Architecture
- Check [Troubleshooting Guide](./TROUBLESHOOTING.md) for common database issues

---

**Last Updated**: December 2024  
**Version**: 1.0.0

# Troubleshooting Guide

This guide provides solutions to common issues encountered during development, testing, and deployment of the PhotoPixels backend.

---

## Table of Contents

1. [Setup & Installation Issues](#setup--installation-issues)
2. [Build & Compilation Errors](#build--compilation-errors)
3. [Database Issues](#database-issues)
4. [Authentication Errors](#authentication-errors)
5. [File Upload Problems](#file-upload-problems)
6. [Docker & Container Issues](#docker--container-issues)
7. [Testing Problems](#testing-problems)
8. [Production Issues](#production-issues)

---

## Setup & Installation Issues

### GitVersion: Cannot find the .git directory

**Error**:
```
GitVersion: Cannot find the .git directory
```

**Cause**: GitVersion expects a Git repository but `.git` folder is missing or inaccessible.

**Solutions**:

1. **Initialize Git repository**:
   ```powershell
   git init
   git add .
   git commit -m "Initial commit"
   ```

2. **Remove GitVersion dependency** (alternative):
   ```xml
   <!-- SF.PhotoPixels.API.csproj -->
   <!-- Comment out or remove -->
   <PackageReference Include="GitVersion.MsBuild" Version="5.12.0" />
   ```

3. **Set version manually**:
   ```xml
   <PropertyGroup>
     <Version>1.0.0</Version>
   </PropertyGroup>
   ```

### Docker Desktop Not Running

**Error**:
```
Cannot connect to the Docker daemon. Is the docker daemon running?
```

**Cause**: Docker Desktop is not running or not installed.

**Solutions**:

1. **Start Docker Desktop**:
   - Windows: Open Docker Desktop from Start menu
   - macOS: Open Docker Desktop from Applications
   - Linux: `sudo systemctl start docker`

2. **Verify Docker is running**:
   ```powershell
   docker ps
   ```

3. **Install Docker Desktop** (if not installed):
   - Download from [docker.com](https://www.docker.com/products/docker-desktop)

### PostgreSQL Port Already in Use

**Error**:
```
Error starting userland proxy: listen tcp4 0.0.0.0:5432: bind: address already in use
```

**Cause**: Another PostgreSQL instance or application is using port 5432.

**Solutions**:

1. **Change port in docker-compose.yml**:
   ```yaml
   postgres:
     ports:
       - "5433:5432"  # Use 5433 on host
   ```

2. **Update connection string**:
   ```json
   "ConnectionStrings": {
     "PhotoPixels": "Host=localhost;Port=5433;..."
   }
   ```

3. **Stop conflicting PostgreSQL**:
   ```powershell
   # Windows
   Stop-Service postgresql-x64-14

   # Linux/macOS
   sudo systemctl stop postgresql
   ```

### NuGet Package Restore Fails

**Error**:
```
error NU1101: Unable to find package Marten
```

**Cause**: NuGet source not configured or network issue.

**Solutions**:

1. **Clear NuGet cache**:
   ```powershell
   dotnet nuget locals all --clear
   ```

2. **Restore packages explicitly**:
   ```powershell
   dotnet restore
   ```

3. **Check NuGet sources**:
   ```powershell
   dotnet nuget list source
   ```

4. **Add NuGet.org as source**:
   ```powershell
   dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
   ```

---

## Build & Compilation Errors

### CS0234: The type or namespace name does not exist

**Error**:
```
CS0234: The type or namespace name 'Marten' does not exist in the namespace 'SF.PhotoPixels'
```

**Cause**: Missing project reference or package.

**Solutions**:

1. **Verify project references**:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\SF.PhotoPixels.Application\SF.PhotoPixels.Application.csproj" />
     <ProjectReference Include="..\SF.PhotoPixels.Infrastructure\SF.PhotoPixels.Infrastructure.csproj" />
   </ItemGroup>
   ```

2. **Restore NuGet packages**:
   ```powershell
   dotnet restore
   dotnet build
   ```

3. **Clean and rebuild**:
   ```powershell
   dotnet clean
   dotnet build
   ```

### CS0103: The name 'IDocumentSession' does not exist

**Cause**: Missing using statement or Marten package.

**Solution**:

```csharp
using Marten;

public class MyHandler
{
    private readonly IDocumentSession _session;
    
    public MyHandler(IDocumentSession session)
    {
        _session = session;
    }
}
```

### Build Hangs or Very Slow

**Cause**: Large number of files, antivirus scanning, or low resources.

**Solutions**:

1. **Exclude project folder from antivirus**:
   - Add `C:\Work\PhotoPixels` to Windows Defender exclusions

2. **Disable parallel builds** (if unstable):
   ```powershell
   dotnet build -m:1
   ```

3. **Clean obj/bin folders**:
   ```powershell
   Get-ChildItem -Path . -Include bin,obj -Recurse | Remove-Item -Recurse -Force
   ```

---

## Database Issues

### Cannot Connect to Database

**Error**:
```
Npgsql.NpgsqlException: Failed to connect to localhost:5432
```

**Cause**: PostgreSQL not running or wrong connection string.

**Solutions**:

1. **Verify PostgreSQL is running**:
   ```powershell
   # Docker Compose
   docker-compose ps

   # Direct Docker
   docker ps | grep postgres
   ```

2. **Check connection string**:
   ```json
   // appsettings.json
   "ConnectionStrings": {
     "PhotoPixels": "Host=localhost;Port=5432;Database=photopixels;Username=postgres;Password=postgres"
   }
   ```

3. **Test connection manually**:
   ```powershell
   # Using psql
   psql -h localhost -U postgres -d photopixels

   # Using Docker
   docker exec -it photopixels-postgres psql -U postgres -d photopixels
   ```

### Foreign Key Constraint Violation

**Error**:
```
23503: insert or update on table "mt_doc_objectalbum" violates foreign key constraint "fk_objectalbum_object"
```

**Cause**: Attempting to reference a soft-deleted record or non-existent record.

**Solutions**:

1. **Check if referenced object exists**:
   ```csharp
   var obj = await _session.LoadAsync<ObjectProperties>(objectId);
   if (obj == null || obj.IsDeleted())
   {
       return new NotFound();
   }
   ```

2. **Use unique index for soft-deleted tables**:
   ```sql
   -- Migration script
   CREATE UNIQUE INDEX mt_doc_objectproperties_unique_id 
       ON photos.mt_doc_objectproperties(id) 
       WHERE mt_deleted = false;
   ```

3. **Verify foreign key setup**:
   ```csharp
   options.Schema.For<ObjectAlbum>()
       .ForeignKey<ObjectProperties>(x => x.ObjectId)
       .ForeignKey<Album>(x => x.AlbumId);
   ```

### Migration Fails to Apply

**Error**:
```
DbUp.Engine.DeploymentException: SQL exception occurred
```

**Cause**: SQL syntax error or conflicting migration.

**Solutions**:

1. **Check migration file for errors**:
   ```sql
   -- Verify SQL syntax
   -- Check for typos, missing semicolons, etc.
   ```

2. **Review migration logs**:
   ```
   docker logs photopixels-api | grep -i migration
   ```

3. **Manually apply migration**:
   ```powershell
   psql -h localhost -U postgres -d photopixels -f src/SF.PhotoPixels.Infrastructure/Migrations/0007.MyMigration.sql
   ```

4. **Rollback if needed**:
   ```powershell
   psql -h localhost -U postgres -d photopixels -f src/SF.PhotoPixels.Infrastructure/Migrations/Rollback/0007.MyMigration.sql
   ```

### Duplicate Hash Error

**Error**:
```
23505: duplicate key value violates unique constraint "mt_doc_objectproperties_idx_hash"
```

**Cause**: Uploading duplicate file (same hash).

**Solution**: This is expected behavior for duplicate detection.

```csharp
// Check for duplicates before inserting
var exists = await _session.Query<ObjectProperties>()
    .AnyAsync(x => x.Hash == fileHash && x.UserId == userId && !x.IsDeleted());

if (exists)
{
    return new Conflict("File already exists");
}
```

### EXIF Metadata Extraction Fails

**Error**:
```
System.Exception: Could not extract EXIF metadata
```

**Cause**: Image has no EXIF data or corrupt EXIF headers.

**Solution**: Handle gracefully, use file dates as fallback.

```csharp
try
{
    var metadata = ImageMetadataReader.ReadMetadata(stream);
    var exifDirectory = metadata.OfType<ExifSubIfdDirectory>().FirstOrDefault();
    
    if (exifDirectory?.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var takenDate) == true)
    {
        obj.TakenDate = takenDate;
    }
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Could not extract EXIF metadata, using file creation date");
    obj.TakenDate = obj.CreatedDate;
}
```

---

## Authentication Errors

### 401 Unauthorized

**Error**: HTTP 401 Unauthorized

**Causes**:
1. Missing or invalid JWT token
2. Expired token
3. Token signature verification failed

**Solutions**:

1. **Check token in request**:
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

2. **Verify token is valid**:
   - Use [jwt.io](https://jwt.io) to decode token
   - Check expiration (`exp` claim)
   - Verify signature with secret

3. **Get fresh token**:
   ```powershell
   $response = Invoke-RestMethod -Uri "http://localhost:8080/api/auth/login" `
       -Method POST `
       -Body (@{email="test@example.com"; password="password"} | ConvertTo-Json) `
       -ContentType "application/json"
   
   $token = $response.token
   ```

4. **Check JWT configuration**:
   ```json
   // appsettings.json
   "Jwt": {
     "Secret": "must-be-at-least-64-characters-long-secret-key",
     "Issuer": "PhotoPixels",
     "Audience": "PhotoPixels"
   }
   ```

### 403 Forbidden

**Error**: HTTP 403 Forbidden

**Cause**: User authenticated but lacks required permissions.

**Solutions**:

1. **Check user role**:
   ```csharp
   // Endpoint requires Admin role
   [Authorize(Roles = "Admin")]
   
   // But user has "User" role
   ```

2. **Verify resource ownership**:
   ```csharp
   var album = await _session.LoadAsync<Album>(albumId);
   if (album.UserId != currentUserId)
   {
       return new Forbidden();
   }
   ```

3. **Check policy requirements**:
   ```csharp
   // Program.cs
   builder.Services.AddAuthorization(options =>
   {
       options.AddPolicy("CanManagePhotos", policy =>
           policy.RequireClaim("permission", "photos.manage"));
   });
   ```

### Invalid Credentials

**Error**: HTTP 400 Bad Request - "Invalid credentials"

**Cause**: Wrong email or password.

**Solutions**:

1. **Verify user exists**:
   ```csharp
   var user = await _session.Query<User>()
       .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted());
   ```

2. **Check password hash**:
   ```csharp
   if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
   {
       return new Unauthorized("Invalid credentials");
   }
   ```

3. **Reset password** (if needed):
   ```powershell
   # In psql
   UPDATE photos.mt_doc_user 
   SET data = jsonb_set(data, '{PasswordHash}', '"$2a$11$newhashere"')
   WHERE data->>'Email' = 'user@example.com';
   ```

---

## File Upload Problems

### 413 Payload Too Large

**Error**: HTTP 413 Request Entity Too Large

**Cause**: File exceeds max request size.

**Solutions**:

1. **Increase limit in Program.cs**:
   ```csharp
   builder.Services.Configure<FormOptions>(options =>
   {
       options.MultipartBodyLengthLimit = 104857600; // 100MB
   });
   
   builder.Services.Configure<KestrelServerOptions>(options =>
   {
       options.Limits.MaxRequestBodySize = 104857600; // 100MB
   });
   ```

2. **Increase Nginx limit**:
   ```nginx
   client_max_body_size 100M;
   ```

### 415 Unsupported Media Type

**Error**: HTTP 415 Unsupported Media Type

**Cause**: File type not allowed.

**Solution**: Check allowed media types.

```csharp
// MediaValidator.cs
private static readonly string[] AllowedMimeTypes =
{
    "image/jpeg",
    "image/jpg",
    "image/png",
    "image/gif",
    "image/webp",
    "image/heic",
    "video/mp4",
    "video/quicktime"
};
```

### 507 Insufficient Storage

**Error**: HTTP 507 Insufficient Storage - "Quota exceeded"

**Cause**: User has exceeded their quota.

**Solutions**:

1. **Check current quota usage**:
   ```csharp
   var user = await _session.LoadAsync<User>(userId);
   var available = user.Quota - user.UsedQuota;
   ```

2. **Increase user quota** (if admin):
   ```csharp
   user.Quota = 10_000_000_000; // 10GB
   _session.Update(user);
   await _session.SaveChangesAsync();
   ```

3. **Delete old files**:
   ```csharp
   // Empty trash to free space
   var trashed = await _session.Query<ObjectProperties>()
       .Where(x => x.UserId == userId && x.IsDeleted())
       .ToListAsync();
   
   foreach (var obj in trashed)
   {
       _session.HardDelete(obj);
   }
   await _session.SaveChangesAsync();
   ```

### File Hash Mismatch

**Error**: Custom error - "File hash mismatch"

**Cause**: File was corrupted during upload.

**Solutions**:

1. **Retry upload**: Temporary network issue

2. **Check hash calculation**:
   ```csharp
   using var sha256 = SHA256.Create();
   var hashBytes = await sha256.ComputeHashAsync(fileStream);
   var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
   ```

3. **Verify file integrity**: Re-download or use different source

---

## Docker & Container Issues

### Container Exits Immediately

**Error**: Container starts then exits immediately.

**Cause**: Application crash on startup.

**Solutions**:

1. **Check container logs**:
   ```powershell
   docker logs photopixels-api
   ```

2. **Run container interactively**:
   ```powershell
   docker run -it --entrypoint /bin/bash photopixels-api
   ```

3. **Common causes**:
   - Missing environment variables
   - Cannot connect to database
   - Migration failure

### Cannot Remove Container

**Error**:
```
Error response from daemon: You cannot remove a running container
```

**Solutions**:

```powershell
# Stop then remove
docker stop photopixels-api
docker rm photopixels-api

# Force remove
docker rm -f photopixels-api
```

### Docker Build Fails

**Error**: Build fails during `docker build`

**Solutions**:

1. **Check Dockerfile syntax**
2. **Verify all COPY paths exist**
3. **Clear Docker cache**:
   ```powershell
   docker build --no-cache -t photopixels-api .
   ```

4. **Check Docker disk space**:
   ```powershell
   docker system df
   docker system prune  # Clean up
   ```

### Container Network Issues

**Error**: Container cannot reach other containers or external services.

**Solutions**:

1. **Use Docker network**:
   ```yaml
   # docker-compose.yml
   services:
     api:
       networks:
         - photopixels-net
     postgres:
       networks:
         - photopixels-net
   
   networks:
     photopixels-net:
       driver: bridge
   ```

2. **Use service name as host** (in Docker Compose):
   ```json
   "ConnectionStrings": {
     "PhotoPixels": "Host=postgres;..."  // Not localhost
   }
   ```

---

## Testing Problems

### Tests Fail to Start TestContainers

**Error**:
```
Docker is not running or not accessible
```

**Cause**: Docker Desktop not running.

**Solution**:
1. Start Docker Desktop
2. Wait for Docker to fully start
3. Run tests again

### Test Database Conflicts

**Error**: Tests fail intermittently with database conflicts.

**Cause**: Tests running in parallel sharing database.

**Solution**: Use test collection to run sequentially.

```csharp
[Collection("DatabaseCollection")]
public class UploadTests
{
    // Tests will run sequentially
}
```

### Cannot Connect to Test Database

**Error**: `Npgsql.NpgsqlException` during test.

**Solutions**:

1. **Verify TestContainers config**:
   ```csharp
   _dbContainer = new PostgreSqlBuilder()
       .WithImage("postgres:14")
       .Build();
   ```

2. **Check Docker is running**:
   ```powershell
   docker ps
   ```

3. **Increase timeout** (slow machine):
   ```csharp
   await _dbContainer.StartAsync(CancellationToken.None);
   ```

---

## Production Issues

### High CPU Usage

**Cause**: Inefficient queries, infinite loops, or excessive load.

**Solutions**:

1. **Profile the application**:
   ```powershell
   docker stats photopixels-api
   ```

2. **Check slow queries**:
   ```sql
   -- Enable query logging in PostgreSQL
   ALTER DATABASE photopixels SET log_min_duration_statement = 1000; -- Log queries > 1s
   ```

3. **Optimize queries**:
   - Add indexes on frequently queried columns
   - Use pagination
   - Avoid N+1 queries

4. **Scale horizontally**: Run multiple containers behind load balancer

### High Memory Usage

**Cause**: Memory leaks, large file uploads, or caching.

**Solutions**:

1. **Limit container memory**:
   ```powershell
   docker run --memory="512m" --memory-swap="1g" photopixels-api
   ```

2. **Profile memory usage**:
   ```csharp
   // Enable diagnostic logging
   builder.Logging.AddEventLog();
   ```

3. **Dispose resources properly**:
   ```csharp
   await using var stream = file.OpenReadStream();
   // Stream automatically disposed
   ```

### Database Deadlocks

**Error**:
```
40P01: deadlock detected
```

**Cause**: Concurrent transactions locking same resources.

**Solutions**:

1. **Reduce transaction scope**:
   ```csharp
   // Keep transactions short
   await _session.SaveChangesAsync();
   ```

2. **Use consistent lock order**: Always lock resources in same order

3. **Implement retry logic**:
   ```csharp
   for (int i = 0; i < 3; i++)
   {
       try
       {
           await _session.SaveChangesAsync();
           break;
       }
       catch (PostgresException ex) when (ex.SqlState == "40P01")
       {
           if (i == 2) throw;
           await Task.Delay(100);
       }
   }
   ```

### SSL Certificate Expired

**Error**: HTTPS connection fails.

**Cause**: Let's Encrypt certificate expired (90 days).

**Solution**:

```bash
# Renew certificate
sudo certbot renew

# Reload Nginx
sudo systemctl reload nginx

# Check expiration
sudo certbot certificates
```

### Disk Space Full

**Error**: Application cannot write files or logs.

**Solutions**:

1. **Check disk usage**:
   ```bash
   df -h
   du -sh /var/lib/docker
   ```

2. **Clean up Docker**:
   ```bash
   docker system prune -a --volumes
   ```

3. **Rotate logs**:
   ```bash
   # Configure logrotate
   sudo nano /etc/logrotate.d/photopixels
   ```

4. **Clean old uploads** (if applicable)

---

## Getting Help

If you encounter an issue not covered here:

1. **Check application logs**:
   ```powershell
   docker logs photopixels-api
   ```

2. **Enable debug logging**:
   ```json
   // appsettings.Development.json
   "Logging": {
     "LogLevel": {
       "Default": "Debug"
     }
   }
   ```

3. **Search for error message** in:
   - [Marten Documentation](https://martendb.io/)
   - [Stack Overflow](https://stackoverflow.com/questions/tagged/marten)
   - [GitHub Issues](https://github.com/JasperFx/marten/issues)

4. **Ask for help**:
   - Create GitHub issue with detailed error logs
   - Include steps to reproduce
   - Provide environment details (OS, .NET version, Docker version)

---

**Related Guides**:
- [Getting Started](./GETTING_STARTED.md) - Initial setup help
- [Database Guide](./DATABASE.md) - Database-specific issues
- [Deployment Guide](./DEPLOYMENT.md) - Production deployment issues
- [Testing Guide](./TESTING.md) - Test-related problems

---

**Last Updated**: December 2024  
**Version**: 1.0.0

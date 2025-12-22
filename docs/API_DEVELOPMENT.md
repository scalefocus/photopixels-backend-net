# API Development Guide

## Table of Contents
1. [Creating a New Endpoint](#creating-a-new-endpoint)
2. [Request & Response Patterns](#request--response-patterns)
3. [Route Parameters](#route-parameters)
4. [Authentication & Authorization](#authentication--authorization)
5. [Error Handling](#error-handling)
6. [Validation](#validation)
7. [Best Practices](#best-practices)

---

## Creating a New Endpoint

Follow these steps to create a new API endpoint in PhotoPixels.

### Step 1: Define Request & Response Models

Create your request and response models in the **Application layer**.

**Location**: `SF.PhotoPixels.Application/Query/Album/GetAlbums/`

```csharp
// GetAlbumsRequest.cs
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Query.Album.GetAlbums;

public class GetAlbumsRequest : IRequest<GetAlbumsResponse>
{
    [FromQuery]
    public int Page { get; set; } = 1;
    
    [FromQuery]
    public int PageSize { get; set; } = 20;
    
    [FromQuery]
    public string? SearchTerm { get; set; }
}
```

```csharp
// GetAlbumsResponse.cs
namespace SF.PhotoPixels.Application.Query.Album.GetAlbums;

public class GetAlbumsResponse
{
    public List<AlbumDto> Albums { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class AlbumDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }
    public string? CoverPhotoId { get; set; }
    public int PhotoCount { get; set; }
}
```

### Step 2: Implement the Handler

Create the handler that contains your business logic.

**Location**: `SF.PhotoPixels.Application/Query/Album/GetAlbums/GetAlbumsHandler.cs`

```csharp
using Marten;
using Mediator;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Query.Album.GetAlbums;

public class GetAlbumsHandler : IQueryHandler<GetAlbumsRequest, GetAlbumsResponse>
{
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly ILogger<GetAlbumsHandler> _logger;

    public GetAlbumsHandler(
        IDocumentSession session,
        IExecutionContextAccessor executionContextAccessor,
        ILogger<GetAlbumsHandler> logger)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _logger = logger;
    }

    public async ValueTask<GetAlbumsResponse> Handle(
        GetAlbumsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting albums for user {UserId}, Page {Page}", 
            _executionContextAccessor.UserId, 
            request.Page);

        // Build query
        var query = _session.Query<Album>()
            .Where(x => x.UserId == _executionContextAccessor.UserId);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Name.Contains(request.SearchTerm));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var albums = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var albumDtos = albums.Select(a => new AlbumDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            CreatedDate = a.CreatedDate,
            UpdatedDate = a.UpdatedDate,
            CoverPhotoId = a.CoverPhotoId,
            PhotoCount = a.PhotoCount
        }).ToList();

        return new GetAlbumsResponse
        {
            Albums = albumDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
```

### Step 3: Create the API Endpoint

Create the endpoint in the **API layer**.

**Location**: `SF.PhotoPixels.API/Endpoints/Album/GetAlbumsEndpoint.cs`

```csharp
using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.Album.GetAlbums;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Album;

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
        Description = "Retrieves a paginated list of albums for the authenticated user",
        OperationId = "GetAlbums",
        Tags = new[] { "Album operations" }
    )]
    [ProducesResponseType(typeof(GetAlbumsResponse), 200)]
    [ProducesResponseType(401)]
    public override async Task<ActionResult<GetAlbumsResponse>> HandleAsync(
        [FromQuery] GetAlbumsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}
```

### Step 4: Test Your Endpoint

1. **Run the application**
2. **Open Swagger**: http://localhost:5000/swagger
3. **Find your endpoint** under "Album operations"
4. **Test it** with different parameters

---

## Request & Response Patterns

### Using OneOf for Multiple Response Types

For endpoints that can return different response types (success, error, not found), use `OneOf`:

```csharp
using OneOf;
using OneOf.Types;

// Request
public class GetObjectRequest : IRequest<OneOf<GetObjectResponse, NotFound>>
{
    [FromRoute]
    public string ObjectId { get; set; }
}

// Handler
public async ValueTask<OneOf<GetObjectResponse, NotFound>> Handle(
    GetObjectRequest request,
    CancellationToken cancellationToken)
{
    var obj = await _session.LoadAsync<ObjectProperties>(request.ObjectId, cancellationToken);
    
    if (obj == null)
        return new NotFound();
    
    return new GetObjectResponse
    {
        Id = obj.Id,
        // ... map properties
    };
}

// Endpoint
public override async Task<ActionResult<OneOf<GetObjectResponse, NotFound>>> HandleAsync(
    [FromRoute] GetObjectRequest request,
    CancellationToken cancellationToken = default)
{
    var result = await _mediator.Send(request, cancellationToken);
    
    return result.Match<ActionResult<OneOf<GetObjectResponse, NotFound>>>(
        response => Ok(response),
        notFound => NotFound()
    );
}
```

### Common Response Types

- `Success`: Operation completed successfully (no data)
- `NotFound`: Resource not found (404)
- `Forbidden`: User lacks permission (403)
- `ValidationError`: Input validation failed (400)
- `BusinessLogicError`: Business rule violation (400)

---

## Route Parameters

### From Route (Path Parameters)

Use `[FromRoute]` for parameters in the URL path:

```csharp
// Single parameter
[HttpGet("/album/{albumId}")]
public async Task<ActionResult> GetAlbum(
    [FromRoute] Guid albumId,
    CancellationToken cancellationToken)
{
    // GET /album/123e4567-e89b-12d3-a456-426614174000
}

// Multiple parameters
[HttpGet("/album/{albumId}/photo/{photoId}")]
public async Task<ActionResult> GetPhoto(
    [FromRoute] Guid albumId,
    [FromRoute] Guid photoId)
{
    // GET /album/123.../photo/456...
}

// Optional parameter
[HttpGet("/album/{albumId}/items/{lastId?}")]
public async Task<ActionResult> GetAlbumItems(
    [FromRoute] Guid albumId,
    [FromRoute] Guid? lastId = null)
{
    // GET /album/123.../items
    // GET /album/123.../items/456...
}
```

### From Query (Query String Parameters)

Use `[FromQuery]` for parameters in the query string:

```csharp
[HttpGet("/albums")]
public async Task<ActionResult> GetAlbums(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? searchTerm = null)
{
    // GET /albums?page=2&pageSize=50&searchTerm=vacation
}
```

### From Body (Request Body)

Use `[FromBody]` for JSON in the request body:

```csharp
[HttpPost("/albums")]
public async Task<ActionResult> CreateAlbum(
    [FromBody] CreateAlbumRequest request)
{
    // POST /albums
    // Body: { "name": "Vacation 2025", "description": "..." }
}
```

### Mixed Parameters in a Model

You can combine route and query parameters in a single model:

```csharp
public class GetAlbumItemsRequest
{
    [FromRoute]
    public Guid AlbumId { get; set; }
    
    [FromRoute]
    public Guid? LastId { get; set; }
    
    [FromQuery]
    public int Page { get; set; } = 1;
    
    [FromQuery]
    public int PageSize { get; set; } = 20;
}

[HttpGet("/album/{albumId}/items/{lastId?}")]
public async Task<ActionResult> GetAlbumItems(
    [FromQuery] GetAlbumItemsRequest request)
{
    // GET /album/123.../items?page=2
    // GET /album/123.../items/456...?page=2&pageSize=50
}
```

---

## Authentication & Authorization

### Default Authentication

All endpoints require authentication by default:

```csharp
// Configured in Program.cs
builder.Services.AddAuthorization(options =>
{
    var lockedDown = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.FallbackPolicy = lockedDown;
});
```

### Allow Anonymous Access

Use `[AllowAnonymous]` for public endpoints:

```csharp
[HttpPost("/auth/login")]
[AllowAnonymous]
[SwaggerOperation(
    Summary = "User login",
    Description = "Authenticates a user and returns a JWT token",
    Tags = new[] { "Authentication" }
)]
public async Task<ActionResult<LoginResponse>> Login(
    [FromBody] LoginRequest request)
{
    // Public endpoint - no authentication required
}
```

### Admin-Only Endpoints

Use `[RequireAdminRole]` for admin-only endpoints:

```csharp
[HttpPost("/admin/quota")]
[RequireAdminRole]
[SwaggerOperation(
    Summary = "Adjust user quota",
    Description = "Adjusts storage quota for a user (admin only)",
    Tags = new[] { "Admin" }
)]
public async Task<ActionResult> AdjustQuota(
    [FromBody] AdjustQuotaRequest request)
{
    // Only users with Role.Admin can access this
}
```

### Custom Authorization

For custom authorization logic, create a policy:

```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageAlbum", policy =>
        policy.Requirements.Add(new AlbumOwnerRequirement()));
});

// Use in endpoint
[HttpDelete("/album/{albumId}")]
[Authorize(Policy = "CanManageAlbum")]
public async Task<ActionResult> DeleteAlbum([FromRoute] Guid albumId)
{
    // Only album owner can delete
}
```

### Accessing Current User

Use `IExecutionContextAccessor` to get the current user:

```csharp
public class MyHandler
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    
    public async ValueTask<MyResponse> Handle(...)
    {
        var currentUserId = _executionContextAccessor.UserId;
        var currentUserEmail = _executionContextAccessor.UserEmail;
        
        // Use current user info
    }
}
```

---

## Error Handling

### Using OneOf for Type-Safe Errors

```csharp
public async ValueTask<OneOf<UploadResponse, ValidationError, Forbidden>> Handle(
    UploadRequest request,
    CancellationToken cancellationToken)
{
    // Validation error
    if (request.File.Length == 0)
        return new ValidationError("File cannot be empty");
    
    // Authorization error
    if (!await CanUpload(userId))
        return new Forbidden();
    
    // Success
    return new UploadResponse { ObjectId = objectId };
}
```

### Matching Responses in Endpoint

```csharp
public override async Task<ActionResult<OneOf<UploadResponse, ValidationError, Forbidden>>> HandleAsync(
    UploadRequest request,
    CancellationToken cancellationToken = default)
{
    var result = await _mediator.Send(request, cancellationToken);
    
    return result.Match<ActionResult<OneOf<UploadResponse, ValidationError, Forbidden>>>(
        success => Ok(success),
        validationError => BadRequest(validationError),
        forbidden => Forbid()
    );
}
```

### Global Exception Handling

Unhandled exceptions are caught by `ExceptionHandlingMiddleware`:

```csharp
// In Middlewares/ExceptionHandlingMiddleware.cs
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
    }
}
```

---

## Validation

### FluentValidation

Use FluentValidation for complex validation rules:

```csharp
// CreateAlbumValidator.cs
using FluentValidation;

public class CreateAlbumValidator : AbstractValidator<CreateAlbumRequest>
{
    public CreateAlbumValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Album name is required")
            .MaximumLength(100).WithMessage("Album name must not exceed 100 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
```

### Automatic Validation

Validation runs automatically via the `RequestValidator` pipeline:

```csharp
// In Application/Pipelines/RequestValidator.cs
public class RequestValidator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IValidate
{
    public async ValueTask<TResponse> Handle(...)
    {
        var validationResult = await _validator.ValidateAsync(message, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return (TResponse)(object)new ValidationError(errors);
        }
        
        return await next(message, cancellationToken);
    }
}
```

---

## Best Practices

### 1. Keep Endpoints Thin

Endpoints should only:
- Accept the request
- Send to mediator
- Return the response

```csharp
// ✅ Good
public override async Task<ActionResult<GetAlbumsResponse>> HandleAsync(
    [FromQuery] GetAlbumsRequest request,
    CancellationToken cancellationToken = default)
{
    var result = await _mediator.Send(request, cancellationToken);
    return Ok(result);
}

// ❌ Bad - business logic in endpoint
public override async Task<ActionResult<GetAlbumsResponse>> HandleAsync(...)
{
    var albums = await _session.Query<Album>()
        .Where(x => x.UserId == userId)
        .ToListAsync();
    
    return Ok(new GetAlbumsResponse { Albums = albums });
}
```

### 2. Use DTOs, Not Domain Entities

Never expose domain entities directly:

```csharp
// ✅ Good
public class AlbumDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}

// ❌ Bad - exposing domain entity
[HttpGet("/albums")]
public async Task<ActionResult<List<Album>>> GetAlbums() { }
```

### 3. Use Proper HTTP Status Codes

```csharp
// 200 OK - Success
return Ok(response);

// 201 Created - Resource created
return CreatedAtAction(nameof(GetAlbum), new { id = album.Id }, album);

// 204 No Content - Success with no body
return NoContent();

// 400 Bad Request - Validation error
return BadRequest(validationError);

// 401 Unauthorized - Not authenticated
return Unauthorized();

// 403 Forbidden - Not authorized
return Forbid();

// 404 Not Found - Resource not found
return NotFound();

// 500 Internal Server Error - Unhandled exception
// Handled by ExceptionHandlingMiddleware
```

### 4. Document with Swagger

```csharp
[SwaggerOperation(
    Summary = "Short description",
    Description = "Detailed description with examples",
    OperationId = "UniqueName",
    Tags = new[] { "GroupName" }
)]
[ProducesResponseType(typeof(SuccessResponse), 200)]
[ProducesResponseType(typeof(ErrorResponse), 400)]
[ProducesResponseType(401)]
[ProducesResponseType(404)]
```

### 5. Use Cancellation Tokens

Always pass `CancellationToken` for async operations:

```csharp
public async ValueTask<Response> Handle(
    Request request,
    CancellationToken cancellationToken) // Always include
{
    var data = await _session.Query<Entity>()
        .ToListAsync(cancellationToken); // Pass it along
}
```

### 6. Log Important Actions

```csharp
_logger.LogInformation(
    "User {UserId} uploaded photo {ObjectId}", 
    userId, 
    objectId);

_logger.LogWarning(
    "User {UserId} exceeded quota", 
    userId);

_logger.LogError(ex, 
    "Failed to process image {ObjectId}", 
    objectId);
```

---

## Examples

### Complete Command Example

```csharp
// 1. Request (Application layer)
public class CreateAlbumRequest : IRequest<OneOf<CreateAlbumResponse, ValidationError>>
{
    public string Name { get; set; }
    public string? Description { get; set; }
}

// 2. Response
public class CreateAlbumResponse
{
    public string AlbumId { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}

// 3. Validator
public class CreateAlbumValidator : AbstractValidator<CreateAlbumRequest>
{
    public CreateAlbumValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

// 4. Handler
public class CreateAlbumHandler : IRequestHandler<CreateAlbumRequest, OneOf<CreateAlbumResponse, ValidationError>>
{
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    
    public async ValueTask<OneOf<CreateAlbumResponse, ValidationError>> Handle(
        CreateAlbumRequest request,
        CancellationToken cancellationToken)
    {
        var album = new Album(
            request.Name, 
            request.Description, 
            _executionContextAccessor.UserId);
        
        _session.Insert(album);
        await _session.SaveChangesAsync(cancellationToken);
        
        return new CreateAlbumResponse
        {
            AlbumId = album.Id,
            CreatedDate = album.CreatedDate
        };
    }
}

// 5. Endpoint (API layer)
public class CreateAlbumEndpoint : EndpointBaseAsync
    .WithRequest<CreateAlbumRequest>
    .WithActionResult<OneOf<CreateAlbumResponse, ValidationError>>
{
    private readonly IMediator _mediator;

    [HttpPost("/albums")]
    [SwaggerOperation(Summary = "Create album", Tags = new[] { "Album operations" })]
    public override async Task<ActionResult<OneOf<CreateAlbumResponse, ValidationError>>> HandleAsync(
        [FromBody] CreateAlbumRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        
        return result.Match<ActionResult<OneOf<CreateAlbumResponse, ValidationError>>>(
            response => CreatedAtAction(nameof(GetAlbum), new { id = response.AlbumId }, response),
            validationError => BadRequest(validationError)
        );
    }
}
```

---

**Last Updated**: December 2025  
**Version**: 1.0.0

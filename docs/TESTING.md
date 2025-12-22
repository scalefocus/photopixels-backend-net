# Testing Guide

This guide covers testing strategies, practices, and patterns for the PhotoPixels backend. Learn how to write integration tests, run the test suite, and use test helpers.

---

## Table of Contents

1. [Overview](#overview)
2. [Test Structure](#test-structure)
3. [Integration Tests](#integration-tests)
4. [Test Helpers](#test-helpers)
5. [Writing Tests](#writing-tests)
6. [Running Tests](#running-tests)
7. [Test Patterns](#test-patterns)
8. [Best Practices](#best-practices)

---

## Overview

### Testing Philosophy

PhotoPixels uses **integration testing** as the primary testing strategy:

- **Integration Tests**: Test entire request/response pipeline with real database
- **No Unit Tests**: Avoid mocking; test actual behavior
- **Real Dependencies**: Use TestContainers for PostgreSQL
- **Realistic Scenarios**: Test as close to production as possible

### Why Integration Tests?

1. **Confidence**: Test actual database queries, migrations, and business logic together
2. **Refactoring Safety**: Tests don't break when internal implementation changes
3. **Real Behavior**: Catch integration issues early
4. **Less Maintenance**: No complex mocking infrastructure

### Test Technology Stack

- **Test Framework**: xUnit
- **Test Server**: ASP.NET Core WebApplicationFactory
- **Database**: TestContainers (PostgreSQL in Docker)
- **Assertions**: FluentAssertions
- **HTTP Client**: Built-in HttpClient

---

## Test Structure

### Project Organization

```
SF.PhotoPixels.API.Tests/
├── Fixtures/
│   ├── ApiTestFixture.cs              # Base test setup
│   ├── DatabaseFixture.cs             # Database container setup
│   └── TestAuthHandler.cs             # Mock authentication
├── Helpers/
│   ├── TestDataFactory.cs             # Test data generation
│   ├── HttpClientExtensions.cs        # HTTP helper methods
│   └── AssertionExtensions.cs         # Custom assertions
├── Features/
│   ├── Auth/
│   │   └── LoginTests.cs
│   ├── Media/
│   │   ├── UploadTests.cs
│   │   ├── GetObjectTests.cs
│   │   └── TrashTests.cs
│   └── Albums/
│       ├── CreateAlbumTests.cs
│       └── GetAlbumsTests.cs
└── SF.PhotoPixels.API.Tests.csproj
```

### File Naming Convention

- Test files: `{Feature}Tests.cs` (e.g., `UploadTests.cs`)
- Test methods: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `UploadPhoto_ValidRequest_ReturnsCreated`)

---

## Integration Tests

### Test Fixture Setup

**DatabaseFixture** (shared across tests):

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer _dbContainer = null!;
    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Start PostgreSQL container
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:14")
            .WithDatabase("photopixels_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _dbContainer.StartAsync();

        ConnectionString = _dbContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
```

**ApiTestFixture** (sets up test server):

```csharp
public class ApiTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    public HttpClient Client { get; private set; } = null!;
    public IDocumentSession Session { get; private set; } = null!;

    public ApiTestFixture()
    {
        _dbFixture = new DatabaseFixture();
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.InitializeAsync();

        // Override configuration for tests
        Client = CreateClient();
        
        // Get scoped services
        using var scope = Services.CreateScope();
        Session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override connection string for tests
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:PhotoPixels"] = _dbFixture.ConnectionString,
                ["Jwt:Secret"] = "test-secret-key-for-testing-purposes-only"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Mock authentication for tests
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", options => { });
        });
    }

    public async Task DisposeAsync()
    {
        await _dbFixture.DisposeAsync();
        Client?.Dispose();
    }
}
```

### Test Class Template

```csharp
public class UploadTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public UploadTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task UploadPhoto_ValidJpeg_ReturnsCreated()
    {
        // Arrange
        var testImage = TestDataFactory.CreateTestJpeg();
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(testImage), "file", "test.jpg");

        // Act
        var response = await _client.PostAsync("/api/media/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
        result.Should().NotBeNull();
        result!.ObjectId.Should().NotBeEmpty();
    }
}
```

---

## Test Helpers

### TestDataFactory

Create realistic test data:

```csharp
public static class TestDataFactory
{
    public static Stream CreateTestJpeg(int width = 100, int height = 100)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Blue);

        var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Jpeg);
        stream.Position = 0;
        return stream;
    }

    public static User CreateTestUser(string email = "test@example.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Quota = 1_000_000_000, // 1GB
            UsedQuota = 0,
            Role = "User",
            CreatedDate = DateTimeOffset.UtcNow,
            Deleted = false
        };
    }

    public static Album CreateTestAlbum(Guid userId, string name = "Test Album")
    {
        return new Album
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = "Test album description",
            CreatedDate = DateTimeOffset.UtcNow
        };
    }

    public static ObjectProperties CreateTestObject(
        Guid userId, 
        string hash = "abc123",
        long size = 1024)
    {
        return new ObjectProperties
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Hash = hash,
            Size = size,
            MediaType = "image/jpeg",
            Width = 1920,
            Height = 1080,
            TakenDate = DateTimeOffset.UtcNow,
            CreatedDate = DateTimeOffset.UtcNow,
            Deleted = false
        };
    }
}
```

### HttpClient Extensions

Simplify HTTP requests:

```csharp
public static class HttpClientExtensions
{
    public static async Task<T?> GetFromJsonAsync<T>(
        this HttpClient client, 
        string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value)
    {
        var content = JsonContent.Create(value);
        return await client.PostAsync(requestUri, content);
    }

    public static void SetAuthToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public static void SetTestUser(this HttpClient client, Guid userId, string role = "User")
    {
        client.DefaultRequestHeaders.Add("Test-User-Id", userId.ToString());
        client.DefaultRequestHeaders.Add("Test-User-Role", role);
    }
}
```

### Assertion Extensions

Custom assertions for common scenarios:

```csharp
public static class AssertionExtensions
{
    public static void ShouldBeValidationError(
        this HttpResponseMessage response,
        string fieldName)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problem = response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>()
            .Result;

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey(fieldName);
    }

    public static void ShouldBeNotFound(this HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static void ShouldBeUnauthorized(this HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static void ShouldBeForbidden(this HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
```

---

## Writing Tests

### Anatomy of a Test

```csharp
[Fact]
public async Task CreateAlbum_ValidRequest_ReturnsCreatedAlbum()
{
    // ARRANGE: Set up test data and preconditions
    var user = TestDataFactory.CreateTestUser();
    await _fixture.Session.InsertAsync(user);
    await _fixture.Session.SaveChangesAsync();

    _client.SetTestUser(user.Id);

    var request = new CreateAlbumRequest
    {
        Name = "My Vacation",
        Description = "Photos from Hawaii"
    };

    // ACT: Execute the operation being tested
    var response = await _client.PostAsJsonAsync("/api/albums", request);

    // ASSERT: Verify the results
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    var album = await response.Content.ReadFromJsonAsync<AlbumDto>();
    album.Should().NotBeNull();
    album!.Name.Should().Be("My Vacation");
    album.Description.Should().Be("Photos from Hawaii");
    album.UserId.Should().Be(user.Id);
    album.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

    // Verify database state
    var savedAlbum = await _fixture.Session.LoadAsync<Album>(album.Id);
    savedAlbum.Should().NotBeNull();
    savedAlbum!.Name.Should().Be("My Vacation");
}
```

### Testing Different Scenarios

#### Happy Path (Success)

```csharp
[Fact]
public async Task GetAlbum_ExistingAlbum_ReturnsAlbum()
{
    // Arrange
    var user = await CreateTestUserAsync();
    var album = await CreateTestAlbumAsync(user.Id);
    _client.SetTestUser(user.Id);

    // Act
    var response = await _client.GetAsync($"/api/albums/{album.Id}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<AlbumDto>();
    result!.Id.Should().Be(album.Id);
}
```

#### Not Found

```csharp
[Fact]
public async Task GetAlbum_NonExistentAlbum_ReturnsNotFound()
{
    // Arrange
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);
    var fakeId = Guid.NewGuid();

    // Act
    var response = await _client.GetAsync($"/api/albums/{fakeId}");

    // Assert
    response.ShouldBeNotFound();
}
```

#### Validation Error

```csharp
[Fact]
public async Task CreateAlbum_EmptyName_ReturnsValidationError()
{
    // Arrange
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);

    var request = new CreateAlbumRequest
    {
        Name = "",  // Invalid: empty name
        Description = "Test"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/albums", request);

    // Assert
    response.ShouldBeValidationError("Name");
}
```

#### Authorization

```csharp
[Fact]
public async Task DeleteAlbum_DifferentUser_ReturnsForbidden()
{
    // Arrange
    var owner = await CreateTestUserAsync("owner@test.com");
    var otherUser = await CreateTestUserAsync("other@test.com");
    var album = await CreateTestAlbumAsync(owner.Id);

    _client.SetTestUser(otherUser.Id); // Different user

    // Act
    var response = await _client.DeleteAsync($"/api/albums/{album.Id}");

    // Assert
    response.ShouldBeForbidden();
}
```

#### Edge Cases

```csharp
[Fact]
public async Task UploadPhoto_DuplicateHash_ReturnsConflict()
{
    // Arrange
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);

    // Upload first photo
    var image1 = TestDataFactory.CreateTestJpeg();
    var content1 = CreateMultipartContent(image1, "photo1.jpg");
    var response1 = await _client.PostAsync("/api/media/upload", content1);
    response1.StatusCode.Should().Be(HttpStatusCode.Created);

    // Upload same photo again (same content = same hash)
    var image2 = TestDataFactory.CreateTestJpeg(); // Identical content
    var content2 = CreateMultipartContent(image2, "photo2.jpg");

    // Act
    var response2 = await _client.PostAsync("/api/media/upload", content2);

    // Assert
    response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
}
```

### Testing Authentication

#### Anonymous Endpoints

```csharp
[Fact]
public async Task Login_ValidCredentials_ReturnsToken()
{
    // Arrange
    var user = TestDataFactory.CreateTestUser("test@example.com");
    await _fixture.Session.InsertAsync(user);
    await _fixture.Session.SaveChangesAsync();

    var request = new LoginRequest
    {
        Email = "test@example.com",
        Password = "password123"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/auth/login", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
    result!.Token.Should().NotBeNullOrEmpty();
    result.Email.Should().Be("test@example.com");
}
```

#### Authenticated Endpoints

```csharp
[Fact]
public async Task GetProfile_Authenticated_ReturnsUserProfile()
{
    // Arrange
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);

    // Act
    var response = await _client.GetAsync("/api/auth/profile");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var profile = await response.Content.ReadFromJsonAsync<UserDto>();
    profile!.Id.Should().Be(user.Id);
}

[Fact]
public async Task GetProfile_Unauthenticated_ReturnsUnauthorized()
{
    // Act (no auth header)
    var response = await _client.GetAsync("/api/auth/profile");

    // Assert
    response.ShouldBeUnauthorized();
}
```

#### Admin-Only Endpoints

```csharp
[Fact]
public async Task GetAllUsers_AdminUser_ReturnsUserList()
{
    // Arrange
    var admin = await CreateTestUserAsync(role: "Admin");
    _client.SetTestUser(admin.Id, "Admin");

    // Act
    var response = await _client.GetAsync("/api/admin/users");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public async Task GetAllUsers_RegularUser_ReturnsForbidden()
{
    // Arrange
    var user = await CreateTestUserAsync(role: "User");
    _client.SetTestUser(user.Id, "User");

    // Act
    var response = await _client.GetAsync("/api/admin/users");

    // Assert
    response.ShouldBeForbidden();
}
```

---

## Running Tests

### From Command Line

```powershell
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~UploadTests"

# Run specific test method
dotnet test --filter "Name=UploadPhoto_ValidJpeg_ReturnsCreated"

# Run tests in parallel (faster)
dotnet test --parallel

# Collect code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### From Visual Studio

1. Open **Test Explorer** (Test → Test Explorer)
2. Click **Run All** to run all tests
3. Right-click individual test → **Run** to run one test
4. Right-click test class → **Run** to run all tests in class

### From VS Code

1. Install **C# Dev Kit** extension
2. Tests appear in **Testing** sidebar
3. Click ▶️ icon next to test to run
4. Click **Run All Tests** at top

### Docker Requirement

Tests use **TestContainers**, which requires Docker:

```powershell
# Check Docker is running
docker ps

# If not running, start Docker Desktop
```

---

## Test Patterns

### Pattern: Setup Test Data

```csharp
private async Task<User> CreateTestUserAsync(
    string email = "test@example.com",
    string role = "User")
{
    var user = TestDataFactory.CreateTestUser(email);
    user.Role = role;

    await _fixture.Session.InsertAsync(user);
    await _fixture.Session.SaveChangesAsync();

    return user;
}

private async Task<Album> CreateTestAlbumAsync(
    Guid userId,
    string name = "Test Album")
{
    var album = TestDataFactory.CreateTestAlbum(userId, name);

    await _fixture.Session.InsertAsync(album);
    await _fixture.Session.SaveChangesAsync();

    return album;
}
```

### Pattern: Multipart File Upload

```csharp
private MultipartFormDataContent CreateMultipartContent(
    Stream fileStream,
    string fileName,
    string contentType = "image/jpeg")
{
    var content = new MultipartFormDataContent();
    
    var streamContent = new StreamContent(fileStream);
    streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
    
    content.Add(streamContent, "file", fileName);
    
    return content;
}

[Fact]
public async Task UploadPhoto_ValidFile_ReturnsCreated()
{
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);

    var image = TestDataFactory.CreateTestJpeg();
    var content = CreateMultipartContent(image, "test.jpg");

    var response = await _client.PostAsync("/api/media/upload", content);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Pattern: Testing Pagination

```csharp
[Fact]
public async Task GetAlbums_Pagination_ReturnsCorrectPage()
{
    // Arrange
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);

    // Create 25 albums
    for (int i = 1; i <= 25; i++)
    {
        await CreateTestAlbumAsync(user.Id, $"Album {i}");
    }

    // Act
    var response = await _client.GetAsync("/api/albums?page=2&pageSize=10");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var result = await response.Content.ReadFromJsonAsync<PagedResult<AlbumDto>>();
    result!.Items.Should().HaveCount(10);
    result.Page.Should().Be(2);
    result.TotalCount.Should().Be(25);
    result.TotalPages.Should().Be(3);
}
```

### Pattern: Testing Soft Delete

```csharp
[Fact]
public async Task TrashObject_ValidRequest_SoftDeletesObject()
{
    // Arrange
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);

    var obj = await CreateTestObjectAsync(user.Id);

    // Act
    var response = await _client.DeleteAsync($"/api/media/{obj.Id}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Verify soft delete in database
    var trashedObj = await _fixture.Session.Query<ObjectProperties>()
        .Where(x => x.Id == obj.Id && x.IsDeleted())
        .FirstOrDefaultAsync();

    trashedObj.Should().NotBeNull();
    trashedObj!.Deleted.Should().BeTrue();
    trashedObj.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
}
```

### Pattern: Testing Quota Limits

```csharp
[Fact]
public async Task UploadPhoto_ExceedsQuota_ReturnsInsufficientStorage()
{
    // Arrange
    var user = await CreateTestUserAsync();
    user.Quota = 1000; // 1KB quota
    user.UsedQuota = 900; // Already used 900 bytes

    await _fixture.Session.UpdateAsync(user);
    await _fixture.Session.SaveChangesAsync();

    _client.SetTestUser(user.Id);

    var largeImage = TestDataFactory.CreateTestJpeg(2000, 2000); // > 100 bytes remaining
    var content = CreateMultipartContent(largeImage, "large.jpg");

    // Act
    var response = await _client.PostAsync("/api/media/upload", content);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.InsufficientStorage);
}
```

---

## Best Practices

### 1. Test Independence

Each test should be independent and isolated:

```csharp
// ❌ BAD: Tests share state
private User _sharedUser;

[Fact]
public async Task Test1()
{
    _sharedUser = await CreateTestUserAsync();
    // ...
}

[Fact]
public async Task Test2()
{
    // Depends on Test1 running first
    _client.SetTestUser(_sharedUser.Id);
}

// ✅ GOOD: Each test creates its own data
[Fact]
public async Task Test1()
{
    var user = await CreateTestUserAsync();
    // ...
}

[Fact]
public async Task Test2()
{
    var user = await CreateTestUserAsync();
    // ...
}
```

### 2. Descriptive Test Names

```csharp
// ❌ BAD: Unclear what's being tested
[Fact]
public async Task Test1() { }

// ✅ GOOD: Clear scenario and expected outcome
[Fact]
public async Task UploadPhoto_ValidJpeg_ReturnsCreated() { }

[Fact]
public async Task UploadPhoto_ExceedsQuota_ReturnsInsufficientStorage() { }
```

### 3. Arrange-Act-Assert Pattern

```csharp
[Fact]
public async Task CreateAlbum_ValidRequest_ReturnsCreatedAlbum()
{
    // ARRANGE: Setup
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);
    var request = new CreateAlbumRequest { Name = "Test" };

    // ACT: Execute
    var response = await _client.PostAsJsonAsync("/api/albums", request);

    // ASSERT: Verify
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### 4. One Assertion per Test (When Possible)

```csharp
// ❌ BAD: Testing multiple unrelated things
[Fact]
public async Task TestEverything()
{
    // Test upload
    var uploadResponse = await _client.PostAsync(...);
    uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    // Test get
    var getResponse = await _client.GetAsync(...);
    getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    // Test delete
    var deleteResponse = await _client.DeleteAsync(...);
    deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
}

// ✅ GOOD: Separate tests
[Fact]
public async Task UploadPhoto_ValidRequest_ReturnsCreated() { }

[Fact]
public async Task GetPhoto_ExistingPhoto_ReturnsOk() { }

[Fact]
public async Task DeletePhoto_ExistingPhoto_ReturnsNoContent() { }
```

### 5. Test Edge Cases

```csharp
[Theory]
[InlineData("")] // Empty
[InlineData(" ")] // Whitespace
[InlineData("a")] // Too short
[InlineData("ThisNameIsWayTooLongAndExceedsTheMaximumLengthAllowedForAlbumNames")] // Too long
public async Task CreateAlbum_InvalidName_ReturnsValidationError(string invalidName)
{
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);

    var request = new CreateAlbumRequest { Name = invalidName };

    var response = await _client.PostAsJsonAsync("/api/albums", request);

    response.ShouldBeValidationError("Name");
}
```

### 6. Use Theory for Multiple Test Cases

```csharp
[Theory]
[InlineData("image/jpeg", true)]
[InlineData("image/png", true)]
[InlineData("image/gif", true)]
[InlineData("video/mp4", true)]
[InlineData("application/pdf", false)] // Unsupported
[InlineData("text/plain", false)] // Unsupported
public async Task UploadFile_VariousContentTypes_ReturnsExpectedResult(
    string contentType,
    bool shouldSucceed)
{
    // Arrange
    var user = await CreateTestUserAsync();
    _client.SetTestUser(user.Id);

    var file = TestDataFactory.CreateTestFile(contentType);
    var content = CreateMultipartContent(file, "test.file", contentType);

    // Act
    var response = await _client.PostAsync("/api/media/upload", content);

    // Assert
    if (shouldSucceed)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    else
    {
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }
}
```

### 7. Clean Up Test Data

```csharp
// TestContainers automatically cleans up database after tests
// No manual cleanup needed when using fixtures properly

public class UploadTests : IClassFixture<ApiTestFixture>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Runs before each test
    }

    public async Task DisposeAsync()
    {
        // Runs after each test (if needed)
        // Database is already isolated per test
    }
}
```

---

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [TestContainers Documentation](https://dotnet.testcontainers.org/)
- [ASP.NET Core Testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

---

**Next Steps**:
- Review [API Development Guide](./API_DEVELOPMENT.md) for endpoint patterns to test
- See [Database Guide](./DATABASE.md) for understanding data setup in tests
- Check [Getting Started](./GETTING_STARTED.md) for running tests locally

---

**Last Updated**: December 2024  
**Version**: 1.0.0

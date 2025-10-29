using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using SF.PhotoPixels.Application.Commands.Albums;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.AlbumEndpoints;

public class AddAlbumTests : IntegrationTest
{
    public AddAlbumTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddAlbum_WithValidNameAndAuth_ShouldReturnOkAndAlbumInfo()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new AddAlbumRequest
        {
            Name = "My beautiful test album"
        };

        var response = await _httpClient.PostAsJsonAsync("/album/", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AddAlbumResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeNullOrWhiteSpace();
        body.Name.Should().Be(request.Name);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddAlbum_WithoutAuth_ShouldReturnUnauthorized()
    {
        await RevokeAuthentication();

        var request = new AddAlbumRequest
        {
            Name = "ShouldNotWork"
        };

        var response = await _httpClient.PostAsJsonAsync("/album/", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

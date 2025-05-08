using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SF.PhotoPixels.Application.Query.StateChanges;
using Xunit;


namespace SF.PhotoPixels.API.Integration.Tests.Sync;

public class GetChangesEndpointTests : IntegrationTest
{
    public GetChangesEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetChanges_WithOneChange_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        QueueDirectoryDeletion(token.UserId);

        var image = await UploadImageAsync();

        int revisionId = 1;

        var response = await _httpClient.GetAsync($"/revision/{revisionId}");

        var data = await response.Content.ReadFromJsonAsync<StateChangesResponseDetails>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Added.Values.Should().HaveCount(1);
        data.Added.Keys.First().Should().Be(image.Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetChanges_WithNoImage_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();

        int revisionId = 0;

        var response = await _httpClient.GetAsync($"/revision/{revisionId}");

        var data = await response.Content.ReadFromJsonAsync<StateChangesResponseDetails>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Should().NotBeNull();
        data.Added.Values.Should().HaveCount(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetChanges_WithNoAuthorization_ShouldReturnUnauthorized()
    {
        int revisionId = 0;

        var response = await _httpClient.GetAsync($"/revision/{revisionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

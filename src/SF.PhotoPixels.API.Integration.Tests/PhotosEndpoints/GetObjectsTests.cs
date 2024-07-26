using FluentAssertions;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;

namespace SF.PhotoPixels.API.Integration.Tests.PhotosEndpoints;

public class GetObjectsTests : IntegrationTest
{
    public GetObjectsTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetObjects_WithNoAuthorization_ShouldReturnUnauthorized()
    {
        var response = await _httpClient.GetAsync($"/objects?PageSize=3");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetObjects_WithValidImage_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/objects?PageSize=3");

        var contents = await response.Content.ReadFromJsonAsync<GetObjectsResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        contents.Properties.Should().ContainSingle();
        contents.Properties.First().Id.Should().Be(image.Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetObjects_WithInvalidLastId_ShouldReturnNoContent()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        await UploadImageAsync();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/objects?PageSize=3&LastId=wrongId");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
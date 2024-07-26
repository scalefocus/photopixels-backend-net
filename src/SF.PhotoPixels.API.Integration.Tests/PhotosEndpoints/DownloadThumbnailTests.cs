using FluentAssertions;
using System.Net;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.PhotosEndpoints;

public class DownloadThumbnailTests : IntegrationTest
{
    public DownloadThumbnailTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }    

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DownloadThumbnail_WithInvalidId_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();
        string id = "false_id";

        var response = await _httpClient.GetAsync($"/object/{id}/thumbnail");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DownloadThumbnail_WithValidImageAndNoAuthentication_ShouldReturnUnauthorized()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        await RevokeAuthentication();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}/thumbnail");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DownloadThumbnail_WithOtherUserImage_ShouldReturnInternalServerError()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}/thumbnail");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
using FluentAssertions;
using System.Net;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.PhotosEndpoints;

public class DownloadTests : IntegrationTest
{
    public DownloadTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task Download_WithValidImage_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();
        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Download_WithInvalidId_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();
        string id = "false_id";

        var response = await _httpClient.GetAsync($"/object/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Download_WithValidImageAndNoAuthentication_ShouldReturnUnauthorized()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        await RevokeAuthentication();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Download_WithOtherUserImage_ShouldReturnInternalServerError()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

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
    [Trait("Category", "Integration")]
    public async Task Download_WithValidImage_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();
        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Download_WithInvalidId_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();
        string id = "false_id";

        var response = await _httpClient.GetAsync($"/object/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
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
    [Trait("Category", "Integration")]
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

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Download_WithValidVideo_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var video = await UploadVideoAsync();
        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Fact]
    [Trait("Category", "Integration")]
    public async Task Download_WithValidVideoAndNoAuthentication_ShouldReturnUnauthorized()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var video = await UploadVideoAsync();

        await RevokeAuthentication();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Download_WithOtherUserVideo_ShouldReturnInternalServerError()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var video = await UploadVideoAsync();

        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


}

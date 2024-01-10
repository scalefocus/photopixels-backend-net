using System.Net;
using Xunit;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;
using FluentAssertions;

namespace SF.PhotoPixels.API.Integration.Tests.Sync;

public class DeleteObjectEndpointTests : IntegrationTest
{
    public DeleteObjectEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task DeleteObjectEndpoint_WithValidData_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.DeleteAsync($"/object/{image.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteObjectEndpoint_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        await UploadImageAsync();

        QueueDirectoryDeletion(token.UserId);

        var requestId = "invalid_id";

        var response = await _httpClient.DeleteAsync($"/object/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

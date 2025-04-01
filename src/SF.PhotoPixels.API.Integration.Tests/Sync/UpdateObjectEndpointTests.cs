using FluentAssertions;
using Newtonsoft.Json.Linq;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.UpdateObject;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.Sync;

public class UpdateObjectEndpointTests : IntegrationTest
{
    public UpdateObjectEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateObject_WithPhotoValidData_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        var request = new UpdateObjectRequest()
        {
            Id = image.Id,

            RequestBody = new UpdateObjectRequestBody()
            {
                AndroidCloudId = "id",
                AppleCloudId = "id"
            }
        };
        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.PutAsJsonAsync($"/object/{image.Id}", request.RequestBody);

        var content = await response.Content.ReadFromJsonAsync<VersioningResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content?.Revision.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateObject_WithInvalidId_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();

        string invalidId = "invalid_image_id";

        var request = new UpdateObjectRequest()
        {
            Id = invalidId,

            RequestBody = new UpdateObjectRequestBody()
            {
                AndroidCloudId = "id",
                AppleCloudId = "id"
            }
        };

        var response = await _httpClient.PutAsJsonAsync($"/object/{invalidId}", request.RequestBody);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateObject_WithNoAuthentication_ShouldReturnUnauthorized()
    {
        string someId = "some_id";

        var request = new UpdateObjectRequest()
        {
            Id = someId,

            RequestBody = new UpdateObjectRequestBody()
            {
                AndroidCloudId = "id",
                AppleCloudId = "id"
            }
        };

        var response = await _httpClient.PutAsJsonAsync($"/object/{someId}", request.RequestBody);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateObject_WithVideoValidData_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var video = await UploadImageAsync();

        var request = new UpdateObjectRequest()
        {
            Id = video.Id,

            RequestBody = new UpdateObjectRequestBody()
            {
                AndroidCloudId = "id",
                AppleCloudId = "id"
            }
        };
        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.PutAsJsonAsync($"/object/{video.Id}", request.RequestBody);

        var content = await response.Content.ReadFromJsonAsync<VersioningResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content?.Revision.Should().Be(2);
    }

}

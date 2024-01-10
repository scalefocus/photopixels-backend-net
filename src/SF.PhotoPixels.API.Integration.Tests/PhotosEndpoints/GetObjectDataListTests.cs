using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectData;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectDataList;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.PhotosEndpoints;

public class GetObjectDataListTests : IntegrationTest
{
    public GetObjectDataListTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {
    }

    [Fact]
    public async Task GetObjectDataList_WithNoAuthorization_ShouldReturnUnauthorized()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        await RevokeAuthentication();

        QueueDirectoryDeletion(token.UserId);

        var request = new GetObjectDataListRequest
        {
            ObjectIds = new() { image.Id },
        };

        var message = new HttpRequestMessage
        {
            Content = JsonContent.Create(request),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/objects/data", UriKind.Relative),
        };

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetObjectDataList_WithValidImage_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        QueueDirectoryDeletion(token.UserId);

        var request = new GetObjectDataListRequest
        {
            ObjectIds = new() { image.Id },
        };

        var message = new HttpRequestMessage
        {
            Content = JsonContent.Create(request),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/objects/data", UriKind.Relative),
        };

        var response = await _httpClient.SendAsync(message);

        var contents = await response.Content.ReadFromJsonAsync<List<ObjectDataResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        contents.Should().ContainSingle();
        contents.First().Id.Should().Be(image.Id);
    }

    [Fact]
    public async Task GetObjectDataList_WithWrongId_ShouldReturnOkWithEmptyList()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new GetObjectDataListRequest
        {
            ObjectIds = new() { "wrong_id" },
        };

        var message = new HttpRequestMessage
        {
            Content = JsonContent.Create(request),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/objects/data", UriKind.Relative),
        };

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var contents = await response.Content.ReadFromJsonAsync<List<ObjectDataResponse>>();

        contents.Should().BeEmpty();
    }
}
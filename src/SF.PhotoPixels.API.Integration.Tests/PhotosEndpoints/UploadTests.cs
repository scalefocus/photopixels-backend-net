using FluentAssertions;
using SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.PhotosEndpoints;

public class UploadTests : IntegrationTest
{
    public UploadTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Upload_WithImage_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        QueueDirectoryDeletion(token.UserId);

        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));

        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");

        var response = await _httpClient.PostAsync("/object", formDataContent);

        var data = await response.Content.ReadFromJsonAsync<StorePhotoResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Revision.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Upload_WithNoAuth_ShouldReturnUnauthorized()
    {
        var response = await _httpClient.PostAsync("/object", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Upload_WithMismathcedHash_ShouldReturnBadRequest()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        QueueDirectoryDeletion(token.UserId);

        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));

        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent("wrong_hash"), "ObjectHash");

        var response = await _httpClient.PostAsync("/object", formDataContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Upload_WithDuplicateImage_ShouldReturnConflict()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        QueueDirectoryDeletion(token.UserId);

        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));

        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");

        var response = await _httpClient.PostAsync("/object", formDataContent);
        var duplicateResponse = await _httpClient.PostAsync("/object", formDataContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Upload_WithMissingUser_ShouldReturnUnauthorized()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        await RevokeAuthentication();

        QueueDirectoryDeletion(token.UserId);

        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));

        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");

        var response = await _httpClient.PostAsync("/object", formDataContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Upload_WithNotEnougnQuota_ShouldReturnBadRequest()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        await AdjustQuotaAsync(Guid.Parse(token.UserId), 1);

        QueueDirectoryDeletion(token.UserId);

        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));

        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");

        var response = await _httpClient.PostAsync("/object", formDataContent);

        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain("QuotaReached");
    }
}

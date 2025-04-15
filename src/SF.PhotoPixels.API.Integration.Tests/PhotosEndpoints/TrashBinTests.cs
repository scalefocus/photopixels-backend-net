using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;
using SF.PhotoPixels.Application.Commands.VideoStorage.StoreVideo;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.PhotosEndpoints;

public class TrashBinTests : IntegrationTest
{
    public TrashBinTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    [Trait("Category", "TrashBin")]
    public async Task MoveToTrash()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        QueueDirectoryDeletion(token.UserId);

        //Upload item
        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");
        var response = await _httpClient.PostAsync("/object", formDataContent);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<StorePhotoResponse>();

        //get object - confirm object is uploaded
        var getObject = await _httpClient.GetAsync($"/objects?pageSize=3");
        getObject.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await getObject.Content.ReadAsStringAsync();
        content.Should().Contain(data.Id.ToString());

        //trash object - confirm object is in trash bin
        var trashResponse = await _httpClient.DeleteAsync($"/object/{data.Id}/trash");
        trashResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //get object - confirm object is not returned in object list.
        getObject = await _httpClient.GetAsync($"/objects?pageSize=3");
        getObject.StatusCode.Should().Be(HttpStatusCode.NoContent);
        content = await getObject.Content.ReadAsStringAsync();
        content.Should().NotContain(data.Id.ToString());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Trash_WithNoAuth_ShouldReturnUnauthorized()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        QueueDirectoryDeletion(token.UserId);

        //Upload item
        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");
        var response = await _httpClient.PostAsync("/object", formDataContent);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<StorePhotoResponse>();

        //get object - confirm object is uploaded
        var getObject = await _httpClient.GetAsync($"/objects?pageSize=3");
        getObject.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await getObject.Content.ReadAsStringAsync();
        content.Should().Contain(data.Id.ToString());

        //trash object - confirm trash cannot be executed without authorization
        await RevokeAuthentication();
        var trashResponse = await _httpClient.DeleteAsync($"/object/{data.Id}/trash");
        trashResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Trash_Untrash_ShouldBeOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        QueueDirectoryDeletion(token.UserId);

        //Upload item
        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");
        var response = await _httpClient.PostAsync("/object", formDataContent);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<StorePhotoResponse>();

        //get object - confirm object is uploaded
        var getObject = await _httpClient.GetAsync($"/objects?pageSize=3");
        getObject.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await getObject.Content.ReadAsStringAsync();
        content.Should().Contain(data.Id.ToString());

        //trash object - confirm trash cannot be executed without authorization
        var trashResponse = await _httpClient.DeleteAsync($"/object/{data.Id}/trash");
        trashResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //get objects - confirm object IS NOT returned in object list.
        getObject = await _httpClient.GetAsync($"/objects?pageSize=3");
        getObject.StatusCode.Should().Be(HttpStatusCode.NoContent);
        content = await getObject.Content.ReadAsStringAsync();
        content.Should().NotContain(data.Id.ToString());

        //now undelete object - restore it
        var untrashContent = JsonContent.Create(new { Id = data.Id.ToString() });
        var untrashResponse = await _httpClient.PostAsync($"/object/trash/remove", untrashContent);
        untrashResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //get objects - confirm object IS returned in object list.
        getObject = await _httpClient.GetAsync($"/objects?pageSize=3");
        getObject.StatusCode.Should().Be(HttpStatusCode.OK);
        content = await getObject.Content.ReadAsStringAsync();
        content.Should().Contain(data.Id.ToString());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Trash_Untrash_NoAuth_ShouldReturnUnauthorized()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        QueueDirectoryDeletion(token.UserId);

        //Upload item
        var formDataContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        formDataContent.Add(imageContent, "File", "image.jpg");
        formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");
        var response = await _httpClient.PostAsync("/object", formDataContent);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<StorePhotoResponse>();

        //get object - confirm object is uploaded
        var getObject = await _httpClient.GetAsync($"/objects?pageSize=3");
        getObject.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await getObject.Content.ReadAsStringAsync();
        content.Should().Contain(data.Id.ToString());

        //trash object - confirm trash cannot be executed without authorization
        var trashResponse = await _httpClient.DeleteAsync($"/object/{data.Id}/trash");
        trashResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //get objects - confirm object IS NOT returned in object list.
        getObject = await _httpClient.GetAsync($"/objects?pageSize=3");
        getObject.StatusCode.Should().Be(HttpStatusCode.NoContent);
        content = await getObject.Content.ReadAsStringAsync();
        content.Should().NotContain(data.Id.ToString());

        //now undelete object - try to restore it without authorization
        await RevokeAuthentication();
        var untrashContent = JsonContent.Create(new { Id = data.Id.ToString() });
        var untrashResponse = await _httpClient.PostAsync($"/object/trash/remove", untrashContent);
        untrashResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // [Fact]
    // [Trait("Category", "Integration")]
    // public async Task Upload_WithDuplicateImage_ShouldReturnConflict()
    // {
    //     var token = await AuthenticateAsSeededAdminAsync();

    //     QueueDirectoryDeletion(token.UserId);

    //     var formDataContent = new MultipartFormDataContent();
    //     var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));

    //     imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

    //     formDataContent.Add(imageContent, "File", "image.jpg");
    //     formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");

    //     var response = await _httpClient.PostAsync("/object", formDataContent);
    //     var duplicateResponse = await _httpClient.PostAsync("/object", formDataContent);

    //     response.StatusCode.Should().Be(HttpStatusCode.OK);
    //     duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    // }

    // [Fact]
    // [Trait("Category", "Integration")]
    // public async Task Upload_WithMissingUser_ShouldReturnUnauthorized()
    // {
    //     var token = await AuthenticateAsSeededAdminAsync();

    //     await RevokeAuthentication();

    //     QueueDirectoryDeletion(token.UserId);

    //     var formDataContent = new MultipartFormDataContent();
    //     var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));

    //     imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

    //     formDataContent.Add(imageContent, "File", "image.jpg");
    //     formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");

    //     var response = await _httpClient.PostAsync("/object", formDataContent);

    //     response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    // }

    // [Fact]
    // [Trait("Category", "Integration")]
    // public async Task UploadPhoto_WithNotEnougnQuota_ShouldReturnBadRequest()
    // {
    //     var token = await AuthenticateAsSeededAdminAsync();

    //     await AdjustQuotaAsync(Guid.Parse(token.UserId), 1);

    //     QueueDirectoryDeletion(token.UserId);

    //     var formDataContent = new MultipartFormDataContent();
    //     var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));

    //     imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

    //     formDataContent.Add(imageContent, "File", "image.jpg");
    //     formDataContent.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");

    //     var response = await _httpClient.PostAsync("/object", formDataContent);

    //     var content = await response.Content.ReadAsStringAsync();

    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     content.Should().Contain("QuotaReached");
    // }

    // [Fact]
    // [Trait("Category", "Integration")]
    // public async Task Upload_WithVideo_ShouldReturnOk()
    // {
    //     var token = await AuthenticateAsSeededAdminAsync();

    //     QueueDirectoryDeletion(token.UserId);

    //     var formDataContent = new MultipartFormDataContent();
    //     var videoContent = new ByteArrayContent(await File.ReadAllBytesAsync(Constants.RunVideoPath));

    //     videoContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

    //     formDataContent.Add(videoContent, "File", "run.mp4");
    //     formDataContent.Add(new StringContent(Constants.RunVideoHash), "ObjectHash");

    //     var response = await _httpClient.PostAsync("/object", formDataContent);

    //     var data = await response.Content.ReadFromJsonAsync<StoreVideoResponse>();

    //     response.StatusCode.Should().Be(HttpStatusCode.OK);
    //     data.Revision.Should().BeGreaterThanOrEqualTo(1);
    // }

    // [Fact]
    // [Trait("Category", "Integration")]
    // public async Task UploadVideo_WithNotEnougnQuota_ShouldReturnBadRequest()
    // {
    //     var token = await AuthenticateAsSeededAdminAsync();

    //     await AdjustQuotaAsync(Guid.Parse(token.UserId), 1);

    //     QueueDirectoryDeletion(token.UserId);

    //     var formDataContent = new MultipartFormDataContent();
    //     var videoContent = new ByteArrayContent(await File.ReadAllBytesAsync(Constants.RunVideoPath));

    //     videoContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

    //     formDataContent.Add(videoContent, "File", "run.mp4");
    //     formDataContent.Add(new StringContent(Constants.RunVideoHash), "ObjectHash");

    //     var response = await _httpClient.PostAsync("/object", formDataContent);

    //     var content = await response.Content.ReadAsStringAsync();

    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     content.Should().Contain("QuotaReached");
    // }
}

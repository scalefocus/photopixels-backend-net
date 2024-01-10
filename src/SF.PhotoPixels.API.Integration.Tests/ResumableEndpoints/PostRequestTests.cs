using FluentAssertions;
using System.Net;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.ResumableEndpoints;

public class PostRequestTests : IntegrationTest
{
    public PostRequestTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task StartUpload_WithValidData_ShouldReturnCreated()
    {
        await AuthenticateAsSeededAdminAsync();

        var message = new HttpRequestMessage()
        {
            Content = new ReadOnlyMemoryContent(null),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        message.Headers.Add("Tus-Resumable", "1.0.0");

        message.Content.Headers.Remove("Content-Type");
        message.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

        message.Headers.Add("Upload-Length", $"15897654");
        message.Headers.Add("Upload-Metadata", "fileExtension cG5n,fileName dG9SZXR1cm4=,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize MTU4OTc2NTQ=,appleId,androidId");

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = response.Headers.GetValues("Location").First().Substring(11);
        await RemoveTusFiles(id);
    }

    [Fact]
    public async Task StartUpload_WithNoAuthorization_ShouldReturnUnauthorized()
    {
        var message = new HttpRequestMessage()
        {
            Content = new ReadOnlyMemoryContent(null),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        message.Headers.Add("Tus-Resumable", "1.0.0");

        message.Content.Headers.Remove("Content-Type");
        message.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

        message.Headers.Add("Upload-Length", $"15897654");
        message.Headers.Add("Upload-Metadata", "fileExtension cG5n,fileName dG9SZXR1cm4=,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize MTU4OTc2NTQ=,appleId,androidId");

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task StartUpload_WithMissingHeaders_ShouldReturn400()
    {
        await AuthenticateAsSeededAdminAsync();

        var message = new HttpRequestMessage()
        {
            Content = new ReadOnlyMemoryContent(null),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        message.Headers.Add("Tus-Resumable", "1.0.0");

        message.Content.Headers.Remove("Content-Type");
        message.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

        message.Headers.Add("Upload-Length", $"15897654");

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StartUpload_WithMissingUploadLength_ShouldReturn400()
    {
        await AuthenticateAsSeededAdminAsync();

        var message = new HttpRequestMessage()
        {
            Content = new ReadOnlyMemoryContent(null),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        message.Headers.Add("Tus-Resumable", "1.0.0");

        message.Content.Headers.Remove("Content-Type");
        message.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

        message.Headers.Add("Upload-Metadata", "fileExtension cG5n,fileName dG9SZXR1cm4=,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize MTU4OTc2NTQ=,appleId,androidId");

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task StartUpload_WithMissingTusHeader_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();

        var message = new HttpRequestMessage()
        {
            Content = new ReadOnlyMemoryContent(null),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        message.Content.Headers.Remove("Content-Type");
        message.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

        message.Headers.Add("Upload-Length", $"15897654");
        message.Headers.Add("Upload-Metadata", "fileExtension cG5n,fileName dG9SZXR1cm4=,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize MTU4OTc2NTQ=,appleId,androidId");

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
    }

    [Theory]
    [InlineData("fileExtension,fileName dG9SZXR1cm4=,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize MTU4OTc2NTQ=,appleId,androidId")]
    [InlineData("fileExtension cG5n,fileName ,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize MTU4OTc2NTQ=,appleId,androidId")]
    [InlineData("fileExtension cG5n,fileName dG9SZXR1cm4=,fileHash,fileSize MTU4OTc2NTQ=,appleId,androidId")]
    [InlineData("fileExtension cG5n,fileName dG9SZXR1cm4=,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize,appleId,androidId")]
    [InlineData("fileExtension cG5n,fileName dG9SZXR1cm4=,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize MTU4OTc2NTQ=,androidId")]
    [InlineData("fileExtension cG5n,fileName dG9SZXR1cm4=,fileHash +IY/zJPCZEvN5P5ZPV2qG/tkwXI=,fileSize MTU4OTc2NTQ=,appleId")]
    public async Task StartUpload_WithMissingMetadataValues_ShouldReturn400(string UploadMetadata)
    {
        await AuthenticateAsSeededAdminAsync();

        var message = new HttpRequestMessage()
        {
            Content = new ReadOnlyMemoryContent(null),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        message.Headers.Add("Tus-Resumable", "1.0.0");

        message.Content.Headers.Remove("Content-Type");
        message.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

        message.Headers.Add("Upload-Length", $"15897654");
        message.Headers.Add("Upload-Metadata", UploadMetadata);

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

}

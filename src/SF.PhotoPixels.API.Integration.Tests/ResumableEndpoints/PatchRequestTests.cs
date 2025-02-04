using FluentAssertions;
using System.Net;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.ResumableEndpoints;

public class PatchRequestTests : IntegrationTest
{
    public PatchRequestTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }
    
    [Fact]
    public async Task Patch_WithCompleteFile_ShouldReturn_NoContent()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        QueueDirectoryDeletion(token.UserId);

        var id = await PostTusWhiteImage();

        var patchMessage = new HttpRequestMessage()
        {
            Content = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath)),
            Method = HttpMethod.Patch,
            RequestUri = new Uri($"/send_data/{id}", UriKind.Relative)
        };
        patchMessage.Headers.Add("Tus-Resumable", "1.0.0");

        patchMessage.Headers.Add("Upload-Offset", "0");

        patchMessage.Content.Headers.Remove("Content-Length");
        patchMessage.Content.Headers.Add("Content-Length", $"631");

        patchMessage.Content.Headers.Remove("Content-Type");
        patchMessage.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

        var response = await _httpClient.SendAsync(patchMessage);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task Patch_WithPartialFile_ShouldReturn_NoContentWithCorrectOffset()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        QueueDirectoryDeletion(token.UserId);

        var id = await PostTusWhiteImage();

        var partialFile = File.ReadAllBytes(Constants.WhiteimagePath).Take(631).ToArray();

        var patchMessage = new HttpRequestMessage()
        {
            Content = new ByteArrayContent(partialFile),
            Method = HttpMethod.Patch,
            RequestUri = new Uri($"/send_data/{id}", UriKind.Relative)
        };

        patchMessage.Headers.Add("Tus-Resumable", "1.0.0");
        patchMessage.Headers.Add("Upload-Offset", "0");

        patchMessage.Content.Headers.Remove("Content-Length");
        patchMessage.Content.Headers.Add("Content-Length", $"631");

        patchMessage.Content.Headers.Remove("Content-Type");
        patchMessage.Content.Headers.Add("Content-Type", "application/offset+octet-stream");

        var response = await _httpClient.SendAsync(patchMessage);

        response.Headers.GetValues("Upload-Offset").First().Should().Be("631");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await RemoveTusFiles(id);
    }

}

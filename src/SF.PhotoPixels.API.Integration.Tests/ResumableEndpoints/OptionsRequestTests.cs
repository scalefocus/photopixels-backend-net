using FluentAssertions;
using System.Net;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.ResumableEndpoints;

public class OptionsRequestTests : IntegrationTest
{
    public OptionsRequestTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task Options_WithAuthentication_ShouldReturnTusHeadersAndNoContentResponse()
    {
        await AuthenticateAsSeededAdminAsync();

        var message = new HttpRequestMessage()
        {
            Method = HttpMethod.Options,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        var response = await _httpClient.SendAsync(message);

        response.Headers.TryGetValues("Tus-Extension", out var extensions);
        response.Headers.TryGetValues("Tus-Checksum-Algorithm", out var hashes);

        extensions.First().Should().Contain("creation,creation-with-upload,checksum,expiration,concatenation");
        hashes.Should().Contain("sha1,md5");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Options_WithNoAuthentication_ShouldReturnUnauthorized()
    {
        var message = new HttpRequestMessage()
        {
            Method = HttpMethod.Options,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

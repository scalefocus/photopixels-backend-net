using FluentAssertions;
using SF.PhotoPixels.Application.Query.User.RefreshToken;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class RefreshTokenEndpointTests : IntegrationTest
{
    public RefreshTokenEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task RefreshToken_WithValidJRT_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        await RevokeAuthentication();

        var request = new RefreshTokenRequest() {
            RefreshToken = token.RefreshToken
        };

        var response = await _httpClient.PostAsJsonAsync("/user/refresh", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidJRT_ShouldReturnForbidden()
    {
        var request = new RefreshTokenRequest()
        {
            RefreshToken = "invalid_refresh_token"
        };

        var response = await _httpClient.PostAsJsonAsync("/user/refresh", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

using FluentAssertions;
using SF.PhotoPixels.Application.Query.User.GetUserInfo;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class GetUserInfoEndpointTests : IntegrationTest
{
    public GetUserInfoEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task GetUserInfo_WithValidData_ShouldReturnOK()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        var response = await _httpClient.GetAsync("/user/info");

        var data = await response.Content.ReadFromJsonAsync<GetUserInfoResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Claims.Should().ContainKey("id");
        data.Claims.Should().Contain("id", token.UserId);
    }

    [Fact]
    public async Task GetUserInfo_WithNoAutorization_ShouldReturnUnauthorized()
    {
        var response = await _httpClient.GetAsync("/user/info");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

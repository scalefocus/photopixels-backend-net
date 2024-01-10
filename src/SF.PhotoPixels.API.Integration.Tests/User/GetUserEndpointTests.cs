using FluentAssertions;
using SF.PhotoPixels.Application.Query.User;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class GetUserEndpointTests : IntegrationTest
{
    public GetUserEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task GetUser_WithSelf_ShouldReturnOK()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        var response = await _httpClient.GetAsync($"/user/{token.UserId}");

        var data = await response.Content.ReadFromJsonAsync<GetUserResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Id.Should().Be(Guid.Parse(token.UserId));
    }

    [Fact]
    public async Task GetUser_WithWrongId_ShouldReturnNoContent()
    {
        await AuthenticateAsSeededAdminAsync();

        var randomId = Guid.NewGuid().ToString();

        var response = await _httpClient.GetAsync($"/user/{randomId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetUser_WithContributorAuthorized_ShouldReturnForbidden()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var user = await SeedDefaultContributorAsync();

        await AuthenticateAsAsync(user.Email, Constants.DefaultContributorCredentials.Password);

        var response = await _httpClient.GetAsync($"/user/{token.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUser_WithNoAuthorization_ShouldReturnUnauthorized()
    {
        var id = Guid.NewGuid().ToString();

        var response = await _httpClient.GetAsync($"/user/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

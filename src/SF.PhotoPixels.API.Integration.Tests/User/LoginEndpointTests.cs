using FluentAssertions;
using SF.PhotoPixels.Application.Query.User.Login;
using SF.PhotoPixels.Application.Security.BearerToken;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class LoginEndpointTests : IntegrationTest
{
    public LoginEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    protected async Task Login_AsAdmin_ShouldReturnOk()
    {
        var loginRequest = new LoginRequest()
        {
            Email = Constants.SeededAdminCredentials.Email,
            Password = Constants.SeededAdminCredentials.Password
        };

        var response = await _httpClient.PostAsJsonAsync("user/login", loginRequest);

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        token.Should().NotBeNull();
    }

    [Fact]
    protected async Task Login_AsContributor_ShouldReturnOk()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();

        var loginRequest = new LoginRequest()
        {
            Email = Constants.DefaultContributorCredentials.Email,
            Password = Constants.DefaultContributorCredentials.Password
        };

        var response = await _httpClient.PostAsJsonAsync("user/login", loginRequest);

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        token.Should().NotBeNull();
    }

    [Fact]
    protected async Task Login_WithWrongPasswrod_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest()
        {
            Email = Constants.SeededAdminCredentials.Email,
            Password = "P@s"
        };

        var response = await _httpClient.PostAsJsonAsync("user/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    protected async Task Login_WithWrongEmail_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest()
        {
            Email = "wrong@mail.com",
            Password = Constants.SeededAdminCredentials.Password
        };

        var response = await _httpClient.PostAsJsonAsync("user/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

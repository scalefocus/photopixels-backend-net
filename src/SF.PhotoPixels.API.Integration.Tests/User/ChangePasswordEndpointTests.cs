using System.Net.Http.Json;
using System.Net;
using Xunit;
using SF.PhotoPixels.Application.Commands.User.ChangePassword;
using FluentAssertions;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class ChangePasswordEndpointTests : IntegrationTest
{
    public ChangePasswordEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {
    }
  

    [Fact]
    public async Task ChangePassword_WithValidInput_ShouldReturnOk()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new ChangePasswordRequest
        {
            OldPassword = Constants.SeededAdminCredentials.Password,
            NewPassword = "P@ssw0rd2",
        };

        var response = await _httpClient.PostAsJsonAsync("/user/changepassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_WithSamePassword_ShouldReturnBadRequest_SamePasswordError()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new ChangePasswordRequest
        {
            OldPassword = Constants.SeededAdminCredentials.Password,
            NewPassword = Constants.SeededAdminCredentials.Password,
        };

        var response = await _httpClient.PostAsJsonAsync("/user/changepassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WithWrongPassword_ShouldReturnBadRequest()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new ChangePasswordRequest
        {
            OldPassword = "Wrong_Password",
            NewPassword = "P@ssw0rd2",
        };

        var response = await _httpClient.PostAsJsonAsync("/user/changepassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WithWeakPassword_ShouldReturnBadRequest()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new ChangePasswordRequest
        {
            OldPassword = Constants.SeededAdminCredentials.Password,
            NewPassword = "weakpassword",
        };

        var response = await _httpClient.PostAsJsonAsync("/user/changepassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new ChangePasswordRequest
        {
            OldPassword = Constants.SeededAdminCredentials.Password,
            NewPassword = "P@ssw0rd2",
        };

        var response = await _httpClient.PostAsJsonAsync("/user/changepassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
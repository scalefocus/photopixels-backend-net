using SF.PhotoPixels.Application.Commands.User.ForgotPassword;
using SF.PhotoPixels.Application.Commands.User.ResetPassword;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using FluentAssertions;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class AdminResetPasswordEndpointTests : IntegrationTest
{
    public AdminResetPasswordEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {
    }

    [Fact]
    public async Task AdminResetPassword_WithValidInfo_ShouldReturnOk()
    {
        await AuthenticateAsSeededAdminAsync();

        var seededUser = await SeedDefaultContributorAsync();

        var request = new AdminResetPasswordRequest
        {
            Email = seededUser.Email,
            Password = "P@ssw0rd2",
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/resetpassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminResetPassword_WithContributorAuthentication_ShouldReturnForbidden()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        var request = new AdminResetPasswordRequest
        {
            Email = Constants.SeededAdminCredentials.Email,
            Password = "P@ssw0rd2",
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/resetpassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminResetPassword_WithWrongEmail_ShouldReturnBadRequest_UserNotFound()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new AdminResetPasswordRequest
        {
            Email = "wrong@mail.com",
            Password = "P@ssw0rd2",
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/resetpassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AdminResetPassword_WithWeakPassword_ShouldReturnBadRequest()
    {
        await AuthenticateAsSeededAdminAsync();

        var seededUser = await SeedDefaultContributorAsync();

        var request = new AdminResetPasswordRequest
        {
            Email = seededUser.Email,
            Password = "weakpassword",
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/resetpassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
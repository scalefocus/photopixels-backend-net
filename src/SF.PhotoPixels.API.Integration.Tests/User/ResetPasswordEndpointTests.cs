using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SF.PhotoPixels.Application.Commands.User.ForgotPassword;
using SF.PhotoPixels.Application.Commands.User.ResetPassword;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class ResetPasswordEndpointTests : IntegrationTest
{
    public ResetPasswordEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {
    }

    private async Task<string> RequestPasswordChange()
    {
        var request = new ForgotPasswordRequest
        {
            Email = Constants.SeededAdminCredentials.Email,
        };

        var message = new HttpRequestMessage
        {
            Content = JsonContent.Create(request),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/user/forgotpassword", UriKind.Relative),
        };

        await _httpClient.SendAsync(message);

        return await GetTotp();
    }

    [Fact]
    public async Task ResetPassword_WithValidTotp_ShouldReturnOk()
    {
        var code = await RequestPasswordChange();

        var request = new ResetPasswordRequest
        {
            Code = code,
            Email = Constants.SeededAdminCredentials.Email,
            Password = Constants.SeededAdminCredentials.Password,
        };

        var response = await _httpClient.PostAsJsonAsync("/user/resetpassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithWrongEmail_ShouldReturnBadRequest_UserNotFound()
    {
        var request = new ResetPasswordRequest
        {
            Code = "123456",
            Email = "wrong@mail.com",
            Password = Constants.SeededAdminCredentials.Password,
        };

        var response = await _httpClient.PostAsJsonAsync("/user/resetpassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidTotp_ShouldReturnBadRequest_InvalidCode()
    {
        var request = new ResetPasswordRequest
        {
            Code = "Invalid_Code",
            Email = Constants.SeededAdminCredentials.Email,
            Password = Constants.SeededAdminCredentials.Password,
        };

        var response = await _httpClient.PostAsJsonAsync("/user/resetpassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithWeakPassword_ShouldReturnBadRequest()
    {
        var code = await RequestPasswordChange();

        var request = new ResetPasswordRequest
        {
            Code = code,
            Email = Constants.SeededAdminCredentials.Email,
            Password = "weakpassword",
        };

        var response = await _httpClient.PostAsJsonAsync("/user/resetpassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
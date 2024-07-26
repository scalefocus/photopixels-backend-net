using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SF.PhotoPixels.Application.Commands.User.ForgotPassword;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class ForgotPasswordEndpointTests : IntegrationTest
{
    public ForgotPasswordEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {
    }

    [Fact]
    public async Task ForgotPassword_WithNoEmail_ShouldReturnNotFound()
    {
        var request = new ForgotPasswordRequest
        {
            Email = "",
        };

        var message = new HttpRequestMessage
        {
            Content = JsonContent.Create(request),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/user/forgotpassword", UriKind.Relative),
        };

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForgotPassword_WithValidData_ShouldReturnOk()
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

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
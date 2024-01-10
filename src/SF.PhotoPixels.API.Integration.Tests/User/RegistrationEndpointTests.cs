using FluentAssertions;
using SF.PhotoPixels.Application.Commands;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class RegistrationEndpointTests : IntegrationTest
{
    public RegistrationEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task Registration_WithValidData_ShouldReturnOK()
    {
        await AuthenticateAsSeededAdminAsync();

        var requestBody = new RegistrationRequest()
        {
            Value = true
        };

        var response = await _httpClient.PostAsJsonAsync("/registration", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Registration_WithContributorAuthentication_ShouldReturnForbidden()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        var requestBody = new RegistrationRequest()
        {
            Value = true
        };

        var response = await _httpClient.PostAsJsonAsync("/registration", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Registration_WithNoAuthentication_ShouldReturnUnauthorized()
    {
        var requestBody = new RegistrationRequest()
        {
            Value = true
        };

        var response = await _httpClient.PostAsJsonAsync("/registration", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

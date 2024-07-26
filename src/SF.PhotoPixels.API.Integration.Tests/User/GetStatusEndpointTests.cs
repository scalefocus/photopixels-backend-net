using System.Net.Http.Json;
using Xunit;
using System.Net;
using FluentAssertions;
using SF.PhotoPixels.Application.Query.GetStatus;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class GetStatusEndpointTests : IntegrationTest
{
    public GetStatusEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetStatus_WithValidData_ShouldReturnOK()
    {
        await AuthenticateAsSeededAdminAsync();

        var response = await _httpClient.GetAsync("/status");

        var data = await response.Content.ReadFromJsonAsync<GetStatusResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.ServerVersion.Should().Contain("1.0.0");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetStatus_WithNoAuth_ShouldReturnOK()
    {
        var response = await _httpClient.GetAsync("/status");

        var data = await response.Content.ReadFromJsonAsync<GetStatusResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.ServerVersion.Should().Contain("1.0.0");
    }
}

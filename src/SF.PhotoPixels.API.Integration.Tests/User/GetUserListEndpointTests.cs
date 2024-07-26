using FluentAssertions;
using SF.PhotoPixels.Application.Query.User;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class GetUserListEndpointTests : IntegrationTest
{
    public GetUserListEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task GetUserList_WithValidData_ShouldReturnSingle()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        var response = await _httpClient.GetAsync("/users");

        var data = await response.Content.ReadFromJsonAsync<List<GetUserResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Should().HaveCount(1);
        data.First().Id.Should().Be(token.UserId);
    }

    [Fact]
    public async Task GetUserList_WithContributorAuthorized_ShouldReturnForbidden()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        var response = await _httpClient.GetAsync("/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserList_WithNoAuthorization_ShouldReturnUnauthorized()
    {
        var response = await _httpClient.GetAsync("/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

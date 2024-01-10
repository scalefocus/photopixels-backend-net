using FluentAssertions;
using System.Net;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class AdminDeleteUserEndpointTests : IntegrationTest
{
    public AdminDeleteUserEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task AdminDeleteUser_WithValidUser_ShouldReturnOK()
    {
        await AuthenticateAsSeededAdminAsync();

        var seededUser = await SeedDefaultContributorAsync();

        var response = await _httpClient.DeleteAsync($"admin/user/{seededUser.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminDeleteUser_WithIdOfAdminItself_ShouldReturnForbidden()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        var seededUser = await SeedDefaultContributorAsync();

        var response = await _httpClient.DeleteAsync($"admin/user/{token.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminDeleteUser_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        await AuthenticateAsSeededAdminAsync();

        var seededUser = await SeedDefaultContributorAsync();

        await RevokeAuthentication();

        var response = await _httpClient.DeleteAsync($"admin/user/{seededUser.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminDeleteUser_WithContributorAuthenticated_ShouldReturnForbidden()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();

        var token = await AuthenticateAsDefaultContributorAsync();

        var response = await _httpClient.DeleteAsync($"admin/user/{token.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(IvalidIds))]
    public async Task AdminDeleteUser_WithMismatchedId_ShouldReturnBadRequest(Guid id)
    {
        await AuthenticateAsSeededAdminAsync();

        var response = await _httpClient.DeleteAsync($"admin/user/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #region Test_Data

    public static TheoryData<Guid> IvalidIds() => new()
    {
        Guid.NewGuid(),
        Guid.Empty
    };

    #endregion
}

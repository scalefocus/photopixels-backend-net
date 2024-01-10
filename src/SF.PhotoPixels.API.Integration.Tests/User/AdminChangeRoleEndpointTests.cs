using FluentAssertions;
using SF.PhotoPixels.Application.Commands.User.ChangeRole;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class AdminChangeRoleEndpointTests : IntegrationTest
{
    public AdminChangeRoleEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task AdminChangeRole_WithValidData_ShouldReturnOk()
    {
        await AuthenticateAsSeededAdminAsync();
        var user =  await SeedDefaultContributorAsync();

        var request = new AdminChangeRoleRequest()
        {
            Id = user.Id,
            Role = Domain.Enums.Role.Admin
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/role", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminChangeRole_WithNoAuthorization_ShouldReturnUnauthorized()
    {
        await AuthenticateAsSeededAdminAsync();
        var user = await SeedDefaultContributorAsync();
        await RevokeAuthentication();

        var request = new AdminChangeRoleRequest()
        {
            Id = user.Id,
            Role = Domain.Enums.Role.Admin
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/role", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminChangeRole_WithInvalidID_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new AdminChangeRoleRequest()
        {
            Id = Guid.NewGuid(),
            Role = Domain.Enums.Role.Admin
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/role", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminChangeRole_WithContributorAuthorized_ShouldReturnForbidden()
    {
        await AuthenticateAsSeededAdminAsync();
        var user = await SeedDefaultContributorAsync();
        await AuthenticateAsAsync(user.Email, Constants.DefaultContributorCredentials.Password);

        var request = new AdminChangeRoleRequest()
        {
            Id = user.Id,
            Role = Domain.Enums.Role.Admin
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/role", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminChangeRole_ChangeRoleForAdminItself_ShouldReturnForbidden()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var user =  await SeedDefaultContributorAsync();

        var request = new AdminChangeRoleRequest()
        {
            Id = new Guid(token.UserId),
            Role = Domain.Enums.Role.Admin
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/role", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

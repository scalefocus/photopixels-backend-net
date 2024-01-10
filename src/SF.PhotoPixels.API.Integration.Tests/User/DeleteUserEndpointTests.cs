using FluentAssertions;
using SF.PhotoPixels.Application.Commands.User.DeleteUser;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class DeleteUserEndpointTests : IntegrationTest
{
    public DeleteUserEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task DeleteUser_WithValidData_ShouldReturnOk()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        var request = new DeleteUserRequest()
        {
            Password = Constants.DefaultContributorCredentials.Password
        };

        var message = new HttpRequestMessage()
        {
            Content = JsonContent.Create(request),
            Method = HttpMethod.Delete,
            RequestUri = new Uri("/user", UriKind.Relative)
        };

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteUser_AsOnlyAdmin_ShouldReturnForbidden()
    {
        await AuthenticateAsSeededAdminAsync();

        var request = new DeleteUserRequest()
        {
            Password = Constants.SeededAdminCredentials.Password
        };

        var message = new HttpRequestMessage()
        {
            Content = JsonContent.Create(request),
            Method = HttpMethod.Delete,
            RequestUri = new Uri("/user", UriKind.Relative)
        };

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_WithWrongPassword_ShouldReturnForbidden()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        var request = new DeleteUserRequest()
        {
            Password = "wrong_P@ssword1"
        };

        var message = new HttpRequestMessage()
        {
            Content = JsonContent.Create(request),
            Method = HttpMethod.Delete,
            RequestUri = new Uri("/user", UriKind.Relative)
        };

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

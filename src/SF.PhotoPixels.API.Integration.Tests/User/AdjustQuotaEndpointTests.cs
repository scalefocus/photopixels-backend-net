using FluentAssertions;
using SF.PhotoPixels.Application.Commands.User.Quota;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class AdjustQuotaEndpointTests : IntegrationTest
{
    public AdjustQuotaEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task AdjustQuota_WithValidData_ShouldReturnOK()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        var requestBody = new AdjustQuotaRequest() {

            Id = Guid.Parse(token.UserId),
            Quota = 100
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);

        var quotaResponse = await response.Content.ReadFromJsonAsync<AdjustQuotaResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        quotaResponse.Quota.Should().Be(requestBody.Quota);
        quotaResponse.UsedQuota.Should().Be(0);
    }

    [Fact]
    public async Task AdjustQuota_WithSameQuota_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        var requestBody = new AdjustQuotaRequest()
        {
            Id = Guid.Parse(token.UserId),
            Quota = 1
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);
        var response2 = await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdjustQuota_WithLowerThanUsedQuota_ShouldReturnBadRequest()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        var requestBody = new AdjustQuotaRequest()
        {
            Id = Guid.Parse(token.UserId),
            Quota = -1
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);

        var content = await response.Content.ReadAsStringAsync();
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain("CannotChangeQuota");
    }

    [Fact]
    public async Task AdjustQuota_WithInvalidUser_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();

        var requestBody = new AdjustQuotaRequest()
        {
            Id = Guid.NewGuid(),
            Quota = 100
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdjustQuota_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var requestBody = new AdjustQuotaRequest()
        {
            Id = Guid.NewGuid(),
            Quota = 100
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdjustQuota_WithContributorAuthentication_ShouldReturnForbidden()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        var requestBody = new AdjustQuotaRequest()
        {
            Id = Guid.NewGuid(),
            Quota = 100
        };

        var response = await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

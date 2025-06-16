using FluentAssertions;
using SF.PhotoPixels.Application.Query.GetStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class HealthEndpointTests : IntegrationTest
{
    public HealthEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task Health_WithAdminAuth_ShouldReturnOk()
    {
        await AuthenticateAsSeededAdminAsync();

        var response = await _httpClient.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_WithNoAuth_ShouldReturnOk()
    {
        var response = await _httpClient.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_WithContributorAuth_ShouldReturnOk()
    {
        await AuthenticateAsSeededAdminAsync();
        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        var response = await _httpClient.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
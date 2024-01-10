﻿using FluentAssertions;
using System.Net;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.PhotosEndpoints;

public class GetObjectDataTests : IntegrationTest
{
    public GetObjectDataTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task GetObjectData_WithValidImage_ShouldReturnOk()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}/data");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetObjectData_WithWrongId_ShouldReturnNotFound()
    {
        await AuthenticateAsSeededAdminAsync();

        var id = "wrong_id";

        var response = await _httpClient.GetAsync($"/object/{id}/data");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetObjectData_WithOtherUserImage_ShouldReturnNotFound()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        await SeedDefaultContributorAsync();
        await AuthenticateAsDefaultContributorAsync();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}/data");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetObjectData_WithNoAuthorization_ShouldReturnUnauthorized()
    {
        var token = await AuthenticateAsSeededAdminAsync();
        var image = await UploadImageAsync();

        await RevokeAuthentication();

        QueueDirectoryDeletion(token.UserId);

        var response = await _httpClient.GetAsync($"/object/{image.Id}/data");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

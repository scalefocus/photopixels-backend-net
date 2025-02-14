﻿using FluentAssertions;
using System.Net;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.ResumableEndpoints;

public class DeleteRequestTests : IntegrationTest
{
    public DeleteRequestTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {
    }

    [Fact]
    public async Task DeleteUpload_WithValidId_ShouldReturnNoContent()
    {
        await AuthenticateAsSeededAdminAsync();

        var id = await PostTusWhiteImage();

        var message = new HttpRequestMessage()
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"/send_data/{id}", UriKind.Relative)
        };

        message.Headers.Add("Tus-Resumable", "1.0.0");

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // If file is not found the DeleteUploadFileInfoAsync considers it has been deleted and returns true
    [Fact]
    public async Task DeleteUpload_WithNonExistingdId_ShouldReturnNoContent()
    {
        await AuthenticateAsSeededAdminAsync();

        var message = new HttpRequestMessage()
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"/send_data/{Guid.NewGuid():N}", UriKind.Relative)
        };

        message.Headers.Add("Tus-Resumable", "1.0.0");

        var response = await _httpClient.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

using FluentAssertions;
using SF.PhotoPixels.Application.Commands;
using SF.PhotoPixels.Application.Commands.User.Register;
using SF.PhotoPixels.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User;

public class UserRegisterEndpointTests : IntegrationTest
{
    public UserRegisterEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
    {

    }

    [Fact]
    public async Task UserRegister_WithBlockedRegistration_ShouldReturn400()
    {
        var requestBody = new UserRegisterRequest()
        {
            Email = "newuser@test.com",
            Name = "newuser@test.com",
            Password = "P@ssword!"
        };

        var response = await _httpClient.PostAsJsonAsync("/user/register", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(ValidRequests))]
    public async Task UserRegister_WithValidRequests_ShouldReturnOk(UserRegisterRequest request)
    {
        await AuthenticateAsSeededAdminAsync();

        var registration = new RegistrationRequest()
        {
            Value = true
        };

        await _httpClient.PostAsJsonAsync("/registration", registration);

        var requestBody = request;

        var response = await _httpClient.PostAsJsonAsync("/user/register", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task UserRegister_WithInvalidRequests_ShouldReturn400(UserRegisterRequest request)
    {
        await AuthenticateAsSeededAdminAsync();

        var registration = new RegistrationRequest()
        {
            Value = true
        };

        await _httpClient.PostAsJsonAsync("/registration", registration);

        var requestBody = request;

        var response = await _httpClient.PostAsJsonAsync("/user/register", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #region Test_Data

    public static IEnumerable<object[]> ValidRequests()
    {
        var testData = new List<UserRegisterRequest>()
            {
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@ssword1" },
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@ssword1" },
            };

        return testData.Select(request => new object[] { request });
    }

    public static IEnumerable<object[]> InvalidRequests()
    {
        var testData = new List<UserRegisterRequest>()
            {
                //Empty Email
                new (){ Email = "", Name = "testUser@test.com", Password = "P@ssword1" },
                //Password missing a number
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@ssword!" },
                //Password less than 8 symbols
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@s1" },
                //Empty Password
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "" },
            };

        return testData.Select(request => new object[] { request });
    }

    #endregion
}

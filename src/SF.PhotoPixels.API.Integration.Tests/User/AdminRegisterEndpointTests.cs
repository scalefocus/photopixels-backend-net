using FluentAssertions;
using SF.PhotoPixels.Application.Commands.User.Register;
using SF.PhotoPixels.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.User
{
    public class AdminRegisterEndpointTests : IntegrationTest
    {
        public AdminRegisterEndpointTests(PhotosWebApplicationFactory customWebFactory) : base(customWebFactory)
        {

        }

        [Theory]
        [MemberData(nameof(ValidRequests))]
        public async Task AdminRegister_WithValidRequest_ShouldReturnOK(AdminRegisterRequest registerRequest)
        {
            await AuthenticateAsSeededAdminAsync();

            var response = await _httpClient.PostAsJsonAsync("admin/register", registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [MemberData(nameof(InvalidRequests))]
        public async Task AdminRegister_WithInvalidRequest_ShouldReturnBadRequest(AdminRegisterRequest registerRequest)
        {
            await AuthenticateAsSeededAdminAsync();

            var response = await _httpClient.PostAsJsonAsync("admin/register", registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #region Test_Data

        public static IEnumerable<object[]> ValidRequests()
        {
            var testData = new List<AdminRegisterRequest>()
            {
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@ssword1", Role = Role.Contributor },
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@ssword1", Role = Role.Admin },
            };

            return testData.Select(request => new object[] { request });
        }

        public static IEnumerable<object[]> InvalidRequests()
        {
            var testData = new List<AdminRegisterRequest>()
            {
                //Contributor role

                //Empty Email
                new (){ Email = "", Name = "testUser@test.com", Password = "P@ssword1", Role = Role.Contributor },
                //Password missing a number
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@ssword!", Role = Role.Contributor },
                //Password less than 8 symbols
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@s1", Role = Role.Contributor },
                //Empty Password
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "", Role = Role.Contributor },

                //Admin role

                //Empty Email
                new (){ Email = "", Name = "testUser@test.com", Password = "P@ssword1", Role = Role.Admin },
                //Password missing a number
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@ssword!", Role = Role.Admin },
                //Password less than 8 symbols
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "P@s1", Role = Role.Admin },
                //Empty Password
                new (){ Email = "testUser@test.com", Name = "testUser@test.com", Password = "", Role = Role.Admin },
            };

            return testData.Select(request => new object[] { request });
        }

        #endregion
    }
}

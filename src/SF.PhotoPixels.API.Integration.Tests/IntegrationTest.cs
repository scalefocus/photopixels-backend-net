using System.Net.Http.Headers;
using System.Net.Http.Json;
using SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;
using SF.PhotoPixels.Application.Commands.User.Quota;
using SF.PhotoPixels.Application.Commands.User.Register;
using SF.PhotoPixels.Application.Commands.VideoStorage.StoreVideo;
using SF.PhotoPixels.Application.Query.User;
using SF.PhotoPixels.Application.Query.User.Login;
using SF.PhotoPixels.Application.Security.BearerToken;
using SF.PhotoPixels.Domain.Enums;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests;

public class IntegrationTest : IClassFixture<PhotosWebApplicationFactory>, IAsyncLifetime
{
    private readonly PhotosWebApplicationFactory _factory;
    protected HttpClient _httpClient;
    private List<Guid> _userDirectoriesToDelete;

    public IntegrationTest(PhotosWebApplicationFactory customWebFactory)
    {
        _factory = customWebFactory;
        _httpClient = _factory.HttpClient;
    }

    public async Task InitializeAsync()
    {
        _userDirectoriesToDelete = new();
        await _factory.InitializeFirstTimeSetup();
    }

    public async Task DisposeAsync()
    {
        await RevokeAuthentication();
        await _factory.ResetDb();
        await CleanUpDirectories();
    }

    protected async Task<AccessTokenResponse> AuthenticateAsSeededAdminAsync()
    {
        var token = await AuthenticateAsAsync(Constants.SeededAdminCredentials.Email, Constants.SeededAdminCredentials.Password);

        return token;
    }

    protected async Task<AccessTokenResponse> AuthenticateAsDefaultContributorAsync()
    {
        var token = await AuthenticateAsAsync(Constants.DefaultContributorCredentials.Email, Constants.DefaultContributorCredentials.Password);

        return token;
    }

    protected async Task<AccessTokenResponse> AuthenticateAsAsync(string email, string password)
    {
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password,
        };

        var response = await _httpClient.PostAsJsonAsync("user/login", loginRequest);

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        return token!;
    }

    protected Task RevokeAuthentication()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        return Task.CompletedTask;
    }

    protected async Task<StorePhotoResponse> UploadImageAsync()
    {
        var multipartFormData = new MultipartFormDataContent();

        var imageContent = new ByteArrayContent(File.ReadAllBytes(Constants.WhiteimagePath));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        multipartFormData.Add(imageContent, "File", "image.jpg");
        multipartFormData.Add(new StringContent(Constants.WhiteimageHash), "ObjectHash");

        var response = await _httpClient.PostAsync("/object", multipartFormData);

        var data = await response.Content.ReadFromJsonAsync<StorePhotoResponse>();

        return data;
    }

    protected async Task<StoreVideoResponse> UploadVideoAsync()
    {
        var multipartFormData = new MultipartFormDataContent();

        var videoContent = new ByteArrayContent(await File.ReadAllBytesAsync(Constants.RunVideoPath));
        videoContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        multipartFormData.Add(videoContent, "File", "run.mp4");
        multipartFormData.Add(new StringContent(Constants.RunVideoHash), "ObjectHash");

        var response = await _httpClient.PostAsync("/object", multipartFormData);

        var data = await response.Content.ReadFromJsonAsync<StoreVideoResponse>();

        return data;
    }

    protected async Task<GetUserResponse> SeedDefaultContributorAsync()
    {
        var registerRequest = new AdminRegisterRequest
        {
            Email = Constants.DefaultContributorCredentials.Email,
            Name = Constants.DefaultContributorCredentials.Name,
            Password = Constants.DefaultContributorCredentials.Password,
            Role = Role.Contributor,
        };

        await _httpClient.PostAsJsonAsync("admin/register", registerRequest);

        var getUsersPesponse = await _httpClient.GetFromJsonAsync<List<GetUserResponse>>("/users");
        var user = getUsersPesponse.Find(u => registerRequest.Email.Equals(u.Email));

        var getUserResponse = await _httpClient.GetFromJsonAsync<GetUserResponse>($"/user/{user.Id}");

        return getUserResponse;
    }

    protected async Task DeleteUserAsync(Guid userId)
    {
        await _httpClient.DeleteAsync($"admin/user/{userId}");
    }

    protected async Task AdjustQuotaAsync(Guid userId, long quota)
    {
        var requestBody = new AdjustQuotaRequest
        {
            Id = userId,
            Quota = quota,
        };

        await _httpClient.PostAsJsonAsync("/admin/quota", requestBody);
    }

    protected async Task<string> GetTotp()
    {
        return await _factory.GetTotp();
    }

    protected void QueueDirectoryDeletion(string userId)
    {
        if (Guid.TryParse(userId, out var id))
        {
            _userDirectoriesToDelete.Add(id);
        }
    }
    protected async Task RemoveTusFiles(string id)
    {
        await AuthenticateAsSeededAdminAsync();

        var message = new HttpRequestMessage()
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"/send_data/{id}", UriKind.Relative)
        };
        message.Headers.Add("Tus-Resumable", "1.0.0");

        await _httpClient.SendAsync(message);
    }
    protected async Task<string> PostTusWhiteImage()
    {
        var token = await AuthenticateAsSeededAdminAsync();

        QueueDirectoryDeletion(token.UserId);

        var message = new HttpRequestMessage()
        {
            Content = new ReadOnlyMemoryContent(null),
            Method = HttpMethod.Post,
            RequestUri = new Uri("/create_upload", UriKind.Relative)
        };

        message.Headers.Add("Tus-Resumable", "1.0.0");
        message.Content.Headers.Remove("Content-Type");
        message.Content.Headers.Add("Content-Length", $"0");

        message.Headers.Add("Upload-Length", $"631");
        message.Headers.Add("Upload-Metadata", "fileExtension anBn,fileName dG9SZXR1cm4=,fileHash YWJjWGlmT2xHaGlIY1Z5ZkR2REdPc0dBTkU0PQ==,fileSize NjMx,appleId,androidId");

        var response = await _httpClient.SendAsync(message);

        return response.Headers.GetValues("Location").First().Replace("send_data", string.Empty).TrimStart('/');
    }
    private async Task CleanUpDirectories()
    {
        await _factory.RemoveUserDirectories(_userDirectoriesToDelete);
        _userDirectoriesToDelete = new List<Guid>();
    }
}
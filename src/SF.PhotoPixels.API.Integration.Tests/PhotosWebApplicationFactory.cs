using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using SF.PhotoPixels.API.Integration.Tests.FakeServices;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Infrastructure.Helpers;
using SF.PhotoPixels.Infrastructure.Storage;
using Testcontainers.PostgreSql;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests;

public class PhotosWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string Username = "postgres";
    private const string Password = "admin";
    private const string Database = "PhotosMetadata";
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly int _port = Random.Shared.Next(4000, 10000);
    private NpgsqlConnection? _dbConnection;
    private Respawner? _respawner;

    public HttpClient? HttpClient { get; private set; }

    public PhotosWebApplicationFactory()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:14.3")
            .WithDatabase(Database)
            .WithUsername(Username)
            .WithPassword(Password)
            .WithPortBinding(_port, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        HttpClient = CreateClient();
        await CreateConnectionToDb();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
    }

    private string ConnectionString()
    {
        return $"Host=localhost;Port={_port};User Id={Username};Password={Password};Database={Database};";
    }

    internal async Task ResetDb()
    {
        await _respawner!.ResetAsync(_dbConnection!);
    }

    internal async Task InitializeFirstTimeSetup()
    {
        await using var scope = Services.CreateAsyncScope();

        var initializationService = scope.ServiceProvider.GetRequiredService<Initialization>();

        await initializationService.FirstTimeSetup();
    }

    internal async Task<string> GetTotp()
    {
        await using var scope = Services.CreateAsyncScope();

        var state = scope.ServiceProvider.GetRequiredService<TotpState>();

        return state.Totp;
    }

    internal async Task RemoveUserDirectories(IEnumerable<Guid> userIds)
    {
        await using var scope = Services.CreateAsyncScope();

        var localObjectStorage = scope.ServiceProvider.GetRequiredService<IObjectStorage>();

        foreach (var id in userIds)
        {
            localObjectStorage.DeleteUserFolders(id);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var inMemConfig = new List<KeyValuePair<string, string?>>
        {
            new("ConnectionStrings:PhotosMetadata", ConnectionString()),
            new("Admin:Email", Constants.SeededAdminCredentials.Email),
            new("Admin:Password", Constants.SeededAdminCredentials.Password),

            new("EmailConfiguration:Host", "mail.example.com"),
            new("EmailConfiguration:Port", "587"),
            new("EmailConfiguration:Username", "sender@example.com"),
            new("EmailConfiguration:Password", ""),
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemConfig).Build();

        builder.UseConfiguration(config);

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TotpState>();
            services.AddTransient<IEmailGenerator, FakeEmailGenerator>();
            services.AddTransient<IEmailSender, FakeEmailSender>();
        });
    }

    private async Task CreateConnectionToDb()
    {
        _dbConnection = new NpgsqlConnection(ConnectionString());

        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "photos" },
            });
    }
}
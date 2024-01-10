using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SF.PhotoPixels.Domain.Constants;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Enums;

namespace SF.PhotoPixels.Application.Core;

public class Initialization
{
    private readonly IConfiguration _configuration;
    private readonly IDocumentSession _documentSession;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<Initialization> _logger;

    public Initialization(IConfiguration configuration, IDocumentSession documentSession, UserManager<User> userManager, ILogger<Initialization> logger)
    {
        _configuration = configuration;
        _documentSession = documentSession;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task FirstTimeSetup()
    {
        var appConfig = await _documentSession.Query<ApplicationConfiguration>().FirstOrDefaultAsync() ?? new ApplicationConfiguration();
        var isFirstTime = appConfig.GetValue<bool?>(ConfigurationConstants.IsFirstTimeSetup) ?? true;

        if (!isFirstTime)
        {
            return;
        }

        var adminEmail = _configuration.GetValue<string>("Admin:Email");
        var adminPassword = _configuration.GetValue<string>("Admin:Password");

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var user = await _documentSession.Query<User>().FirstOrDefaultAsync(x => x.Email == adminEmail);

        if (user is not null)
        {
            appConfig.SetValue(ConfigurationConstants.IsFirstTimeSetup, false);

            _documentSession.Store(appConfig);

            await _documentSession.SaveChangesAsync();

            return;
        }

        user = new User
        {
            Name = adminEmail,
            Email = adminEmail,
            UserName = adminEmail,
            Role = Role.Admin,
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(user, adminPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));

            _logger.LogError("Failed to create admin user. Errors: {Errors}", errors);

            throw new InitializationException("Failed to create admin user.");
        }

        appConfig.SetValue(ConfigurationConstants.IsFirstTimeSetup, false);

        _documentSession.Store(appConfig);

        await _documentSession.SaveChangesAsync();
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SF.PhotoPixels.Application.VersionMigrations.Versions;

namespace SF.PhotoPixels.Application.VersionMigrations;

public static class MigrationExtensions
{
    public static IServiceCollection RegisterMigrations(this IServiceCollection services)
    {
        services.AddSingleton<MigrationExecutor>();

        return services;
    }

    public static Task ExecuteMigrations(this WebApplication host)
    {
        var executor = host.Services.GetRequiredService<MigrationExecutor>();

        return executor.ExecuteMigrations(host.Lifetime.ApplicationStopping);
    }

    public static async Task Execute(this MigrationVersionBase migration, ExecuteContext context, bool throwOnError = true)
    {
        try
        {
            await migration.Execute(context);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Executing migration for object:[{Name}] failed.", context.ObjectName);

            if (throwOnError)
            {
                throw;
            }
        }
    }
}
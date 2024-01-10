using Marten;
using SF.PhotoPixels.Domain.Constants;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.VersionMigrations.Versions;

[MigrationVersion(2)]
public class WrongStartupFlag : MigrationVersionBase
{
    public override async Task BeforeExecute(BeforeExecuteContext context)
    {
        var appConfig = await context.Session.Query<ApplicationConfiguration>().FirstOrDefaultAsync();

        context.StopExecutionForVersion();

        if (appConfig is null)
        {
            return;
        }

        appConfig.SetValue(ConfigurationConstants.IsFirstTimeSetup, false);

        context.Session.Store(appConfig);
    }
}
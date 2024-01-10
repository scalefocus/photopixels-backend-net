using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.VersionMigrations.Versions;

[MigrationVersion(4)]
public class UpdateSecurityStamp : MigrationVersionBase
{
    public override async Task BeforeExecute(BeforeExecuteContext context)
    {
        var userManager = context.Services.GetRequiredService<UserManager<User>>();

        var users = await userManager.Users.ToListAsync();

        foreach (var user in users)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }

        context.StopExecutionForVersion();
    }
}
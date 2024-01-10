namespace SF.PhotoPixels.Application.VersionMigrations.Versions;

[MigrationVersion(5)]
public class UpdateThumbnailsPreserveSmallestSideMigration : MigrationVersionBase
{
    public override Task BeforeExecute(BeforeExecuteContext context)
    {
        context.DisableFailOnError();

        return Task.CompletedTask;
    }

    public override async Task Execute(ExecuteContext context)
    {
        var image = await context.FormattedImage;

        var thumbnail = image.GetThumbnail();

        await context.SaveThumbnail(thumbnail);
    }
}
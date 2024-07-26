namespace SF.PhotoPixels.Application.VersionMigrations.Versions;

[MigrationVersion(3)]
public class UpdateThumbnailsMigration : MigrationVersionBase
{
    public override async Task Execute(ExecuteContext context)
    {
        var image = await context.FormattedImage;

        var thumbnail = image.GetThumbnail(200, 200);

        await context.SaveThumbnail(thumbnail);
    }
}
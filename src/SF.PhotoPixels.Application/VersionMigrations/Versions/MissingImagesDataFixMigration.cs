using SF.PhotoPixels.Infrastructure;

namespace SF.PhotoPixels.Application.VersionMigrations.Versions;

[MigrationVersion(1)]
public class MissingImagesDataFixMigration : MigrationVersionBase
{
    public override Task BeforeExecute(BeforeExecuteContext context)
    {
        var sql = $"""
                   ;with duplicates as (
                       select data ->> 'ObjectId' as objectId
                       from {Constants.DefaultSchema}.mt_events
                       group by data ->> 'ObjectId'
                       having count(data ->> 'ObjectId') > 2
                   )
                    update {Constants.DefaultSchema}.mt_doc_objectproperties set mt_deleted = false
                    where id in (select objectId from duplicates);
                   """;

        context.Session.QueueSqlCommand(sql);

        context.StopExecutionForVersion();

        return Task.CompletedTask;
    }
}
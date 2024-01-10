namespace SF.PhotoPixels.Application.VersionMigrations.Versions;

public abstract class MigrationVersionBase
{
    public virtual Task BeforeExecute(BeforeExecuteContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task Execute(ExecuteContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task AfterExecute(AfterExecuteContext context)
    {
        return Task.CompletedTask;
    }
}
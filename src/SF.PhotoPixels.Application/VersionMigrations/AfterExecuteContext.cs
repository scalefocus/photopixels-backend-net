using Marten;

namespace SF.PhotoPixels.Application.VersionMigrations;

public class AfterExecuteContext : ExecuteContextBase
{
    public required IDocumentSession Session { get; init; }

    public required IServiceProvider Services { get; init; }
}
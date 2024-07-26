using Marten;

namespace SF.PhotoPixels.Application.VersionMigrations;

public class BeforeExecuteContext : ExecuteContextBase
{
    public required IDocumentSession Session { get; init; }

    public required IServiceProvider Services { get; init; }

    public bool FailOnError { get; private set; } = true;

    public void DisableFailOnError()
    {
        FailOnError = false;
    }
}
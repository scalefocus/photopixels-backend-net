using Microsoft.Extensions.Logging;

namespace SF.PhotoPixels.Application.VersionMigrations;

public abstract class ExecuteContextBase
{
    public ILogger Logger { get; set; }

    public bool IsStopRequested { get; private set; }

    public void StopExecutionForVersion()
    {
        IsStopRequested = true;
    }
}
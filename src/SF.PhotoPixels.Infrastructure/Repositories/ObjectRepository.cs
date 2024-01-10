using Marten;
using Marten.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;

namespace SF.PhotoPixels.Infrastructure.Repositories;

public class ObjectRepository : IObjectRepository
{
    private const int RetryCount = 5;
    private readonly ILogger<ObjectRepository> _logger;

    private readonly IDocumentSession _session;

    public ObjectRepository(IDocumentSession session, ILogger<ObjectRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<long> AddEvent(Guid streamId, object evt, CancellationToken cancellationToken)
    {
        var streamState = await _session.Events.FetchStreamStateAsync(streamId, cancellationToken);

        if (streamState != null)
        {
            await _session.Events.AppendExclusive(streamId, cancellationToken);
        }

        var streamAction = _session.Events.Append(streamId, evt);

        await CreatePolicy().ExecuteAsync(() => _session.SaveChangesAsync(cancellationToken));

        return streamAction.Version;
    }

    private AsyncPolicy CreatePolicy()
    {
        return Policy.Handle<ConcurrencyException>()
            .RetryAsync(RetryCount, (ex, i) => _logger.LogError(ex, "Saving failed. Attempt {AttemptNumber}", i));
    }
}
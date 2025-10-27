using Marten;
using Mediator;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class AlbumStateChangesQueryHandler: IQueryHandler<AlbumStateChangesRequest, QueryResponse<AlbumStateChangesResponseDetails>>
{
    private readonly IDocumentSession _session;

    public AlbumStateChangesQueryHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async ValueTask<QueryResponse<AlbumStateChangesResponseDetails>> Handle(AlbumStateChangesRequest query, CancellationToken cancellationToken)
    {
        var changesResponse = await _session.Events
            .AggregateStreamAsync<AlbumStateChangesResponseDetails>(streamId: query.AlbumId,
                fromVersion: query.RevisionId,
                token: cancellationToken);

        if (changesResponse != null)
        {
            return changesResponse;
        }

        var state = await _session.Events.FetchStreamStateAsync(query.AlbumId, cancellationToken);
        var version = state != null && query.RevisionId > state.Version ? state.Version : 0;

        return new AlbumStateChangesResponseDetails { Id = query.AlbumId, Version = version };
    }
}

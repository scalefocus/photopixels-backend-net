using Marten;
using Mediator;
using SF.PhotoPixels.Application.Core;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class StateChangesQueryHandler : IQueryHandler<StateChangesRequest, QueryResponse<StateChangesResponseDetails>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;

    public StateChangesQueryHandler(IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<QueryResponse<StateChangesResponseDetails>> Handle(StateChangesRequest query, CancellationToken cancellationToken)
    {
        var userId = _executionContextAccessor.UserId;
        var changesResponse = await _session.Events
            .AggregateStreamAsync<StateChangesResponseDetails>(userId, fromVersion: query.RevisionId, token: cancellationToken);

        if (changesResponse != null)
        {
            return changesResponse;
        }

        var state = await _session.Events.FetchStreamStateAsync(userId, cancellationToken);
        var version = state != null && query.RevisionId > state.Version ? state.Version : 0;

        return new StateChangesResponseDetails { Id = userId, Version = version };
    }
}
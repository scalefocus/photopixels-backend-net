using Marten;
using Mediator;
using OneOf.Types;
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
        var changesResponse = await _session.Events
            .AggregateStreamAsync<StateChangesResponseDetails>(_executionContextAccessor.UserId, fromVersion: query.RevisionId, token: cancellationToken);

        if (changesResponse != null)
        {
            return changesResponse;
        }

        var state = await _session.Events.FetchStreamStateAsync(_executionContextAccessor.UserId, cancellationToken);
        if (state != null && query.RevisionId > state.Version)
        {
            return new StateChangesResponseDetails { Id = _executionContextAccessor.UserId, Version = state.Version };
        }

        return new NotFound();
    }
}
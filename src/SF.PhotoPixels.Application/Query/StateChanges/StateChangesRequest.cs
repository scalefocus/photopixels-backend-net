using Mediator;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class StateChangesRequest : IQuery<QueryResponse<StateChangesResponseDetails>>
{
    public long RevisionId { get; set; }
}
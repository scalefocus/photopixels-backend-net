using Mediator;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class AlbumStateChangesRequest : IQuery<QueryResponse<AlbumStateChangesResponseDetails>>
{
    public Guid AlbumId { get; set; }
    public long RevisionId { get; set; }
}

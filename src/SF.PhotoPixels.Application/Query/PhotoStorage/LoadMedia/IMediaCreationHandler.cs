using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia
{
    public interface IMediaCreationHandler
    {
        ValueTask<QueryResponse<LoadMediaResponse>> Handle(ObjectProperties? metadata, CancellationToken cancellationToken);
    }
}

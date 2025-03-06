using SF.PhotoPixels.Domain.Models;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public interface IMediaCreationHandler
{
    ValueTask<QueryResponse<LoadMediaResponse>> Handle(LoadMediaCreationModel model, CancellationToken cancellationToken);
}

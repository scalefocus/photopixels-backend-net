using Mediator;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadThumbnail;

public class LoadThumbnailRequest : IQuery<QueryResponse<Stream>>
{
    public required string ObjectId { get; set; }
}
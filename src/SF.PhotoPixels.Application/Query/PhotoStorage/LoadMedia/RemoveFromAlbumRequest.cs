using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class RemoveToAlbumRequest : IQuery<QueryResponse<RemoveFromAlbumResponse>>
{
    [FromRoute]
    public required Guid ObjectId { get; set; }

    [FromRoute]
    public string? AlbumId { get; set; }
}
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class RemoveFromAlbumResponse
{
    [FromRoute]
    public Guid ObjectId { get; set; }

    [FromRoute]
    public Guid AlbumId { get; set; }
}
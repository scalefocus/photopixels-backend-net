using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class AddToAlbumRequest : IRequest<OneOf<AddToAlbumResponse, NotFound>>
{
    [FromRoute(Name = "objectId")]
    public required string ObjectId { get; set; }

    [FromRoute(Name = "albumId")]
    public required string AlbumId { get; set; }
}
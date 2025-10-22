using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.Album;

public class GetAlbumInfoRequest : IQuery<OneOf<GetAlbumInfoResponse, NotFound>>    
{
    [FromRoute(Name = "albumId")]
    public required string AlbumId { get; set; }
}
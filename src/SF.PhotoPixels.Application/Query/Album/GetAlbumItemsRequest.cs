using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace SF.PhotoPixels.Application.Query.Album;

public class GetAlbumItemsRequest : IQuery<OneOf<GetAlbumItemsResponse, ValidationError>>
{
    [FromRoute(Name = "albumId")]
    public required string AlbumId { get; set; }

    [FromQuery(Name = "lastId")]
    public string LastId { get; set; } = string.Empty;

}
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.AlbumObjects;

public class DeleteAlbumObjectRequest : IRequest<OneOf<Success, ValidationError>>
{
    [FromRoute(Name = "albumId")]
    public required string AlbumId { get; set; }

    [FromBody]
    public required string[] ObjectIds { get; set; }

    public DeleteAlbumObjectRequest(string[] objectIds, string albumId)
    {
        ObjectIds = objectIds;
        AlbumId = albumId;
    }

    // Parameterless constructor for serialization/deserialization
    public DeleteAlbumObjectRequest() { }
}
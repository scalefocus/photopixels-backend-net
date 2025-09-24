using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;

public class AddObjectToAlbumRequest : IRequest<OneOf<Success, ValidationError>>
{
    public required string ObjectId { get; set; }
    public required string AlbumId { get; set; }

    public AddObjectToAlbumRequest(string objectId, string albumId)
    {
        ObjectId = objectId;
        AlbumId = albumId;
    }

    // Parameterless constructor for serialization/deserialization
    public AddObjectToAlbumRequest() { }
}


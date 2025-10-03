using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.Albums;

public class AddAlbumRequest : IRequest<OneOf<AddAlbumResponse, ValidationError>>
{
    public required string Name { get; set; }
    public bool IsSystem { get; set; } = false;

    public AddAlbumRequest(string name, bool isSystem = false)
    {
        Name = name;
        IsSystem = isSystem;
    }

    // Parameterless constructor for serialization/deserialization
    public AddAlbumRequest() { }
}
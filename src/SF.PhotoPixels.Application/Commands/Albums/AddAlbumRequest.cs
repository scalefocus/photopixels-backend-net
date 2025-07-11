using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;

public class AddAlbumRequest : IRequest<OneOf<Success, ValidationError>>
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
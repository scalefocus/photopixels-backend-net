using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.Albums;

public class UpdateAlbumRequest : IRequest<OneOf<Success, ValidationError>>
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public bool IsSystem { get; set; } = false;

    public UpdateAlbumRequest(string id, string name, bool isSystem = false)
    {
        Id = id;
        Name = name;
        IsSystem = isSystem;
    }
    
    public UpdateAlbumRequest() { }
}
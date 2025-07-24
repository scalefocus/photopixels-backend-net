using Mediator;

namespace SF.PhotoPixels.Application.Commands.Album.AddObjectPropertiesToAlbum;

public class AddObjectPropertiesToAlbumRequest : IRequest<AddObjectPropertiesToAlbumResponses>
{
    public required Guid AlbumId { get; set; }
    public required List<string> ObjectPropertiesIds { get; set; }
}
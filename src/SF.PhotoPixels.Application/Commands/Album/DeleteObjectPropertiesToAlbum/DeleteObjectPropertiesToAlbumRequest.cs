using Mediator;
using SF.PhotoPixels.Application.Commands.Album.AddObjectPropertiesToAlbum;

namespace SF.PhotoPixels.Application.Commands.Album.DeleteObjectPropertiesToAlbum;

public class DeleteObjectPropertiesToAlbumRequest : IRequest<DeleteObjectPropertiesToAlbumResponses>
{
    public required Guid AlbumId { get; set; }
    public required List<string> ObjectPropertiesIds { get; set; }
}
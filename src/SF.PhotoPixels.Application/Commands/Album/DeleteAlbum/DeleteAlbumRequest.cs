using Mediator;

namespace SF.PhotoPixels.Application.Commands.Album.DeleteAlbum;

public class DeleteAlbumRequest : IRequest<DeleteAlbumResponses>
{
    public required Guid Id { get; set; }
}
using Mediator;
using SF.PhotoPixels.Application.Commands.Album.DeleteAlbum;

namespace SF.PhotoPixels.Application.Commands.Album.GetAlbum;

public class GetAlbumRequest : IRequest<GetAlbumResponses>
{
    public required Guid Id { get; set; }
}

using Mediator;
using SF.PhotoPixels.Application.Commands.Tus.CreateUpload;

namespace SF.PhotoPixels.Application.Commands.Album.CreateAlbum;

public class CreateAlbumRequest : IRequest<CreateAlbumResponses>
{
    public required string Name { get; set; }
    public List<string>? ObjectPropertiesIds { get; set; }
}
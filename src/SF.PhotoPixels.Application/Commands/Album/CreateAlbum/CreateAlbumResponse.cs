using OneOf;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.Album
{
    public class CreateAlbumResponse
    {
        public required SF.PhotoPixels.Domain.Entities.Album Album { get; set; }
    }

    [GenerateOneOf]
    public partial class CreateAlbumResponses : OneOfBase<CreateAlbumResponse, ValidationError>
    {
    }
}
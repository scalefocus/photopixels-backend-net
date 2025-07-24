using OneOf;

namespace SF.PhotoPixels.Application.Commands.Album.DeleteAlbum
{
    public class DeleteAlbumResponse
    {
    }


    [GenerateOneOf]
    public partial class DeleteAlbumResponses : OneOfBase<DeleteAlbumResponse, ValidationError>
    {
    }
}

namespace SF.PhotoPixels.Application.Commands.Album.GetAlbum
{
    public interface IGetAlbumsHandler
    {
        ValueTask<GetAlbumsResponses> Handle(GetAlbumsRequest request, CancellationToken cancellationToken);
    }
}
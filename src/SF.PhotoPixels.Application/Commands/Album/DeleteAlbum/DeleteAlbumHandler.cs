using Marten;
using Mediator;

namespace SF.PhotoPixels.Application.Commands.Album.DeleteAlbum;

public class DeleteAlbumHandler : IRequestHandler<DeleteAlbumRequest, DeleteAlbumResponses>
{
    private readonly IDocumentSession _session;

    public DeleteAlbumHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async ValueTask<DeleteAlbumResponses> Handle(DeleteAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = await _session.LoadAsync<Domain.Entities.Album>(request.Id, cancellationToken);

        if (album is null)
        {
            return new ValidationError("Id", "Album not found.");
        }

        _session.Delete(album);
        await _session.SaveChangesAsync(cancellationToken);

        return new DeleteAlbumResponse();
    }
}
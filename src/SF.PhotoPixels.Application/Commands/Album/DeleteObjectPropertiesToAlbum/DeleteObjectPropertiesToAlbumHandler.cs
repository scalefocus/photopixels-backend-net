using Marten;
using Mediator;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.Album.DeleteObjectPropertiesToAlbum;

public class DeleteObjectPropertiesToAlbumHandler : IRequestHandler<DeleteObjectPropertiesToAlbumRequest, DeleteObjectPropertiesToAlbumResponses>
{
    private readonly IDocumentSession _session;

    public DeleteObjectPropertiesToAlbumHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async ValueTask<DeleteObjectPropertiesToAlbumResponses> Handle(DeleteObjectPropertiesToAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = await _session.LoadAsync<SF.PhotoPixels.Domain.Entities.Album>(request.AlbumId, cancellationToken);

        if (album is null)
        {
            return new ValidationError("AlbumId", "Album not found.");
        }

        if (request.ObjectPropertiesIds == null || request.ObjectPropertiesIds.Count == 0)
        {
            return new ValidationError("ObjectPropertiesIds", "No object property IDs provided.");
        }

        // remove in album all object properties that match the IDs in request
        album.ObjectPropertiesIds.RemoveAll(id => request.ObjectPropertiesIds.Contains(id));
        _session.Store(album);

        // Fix: Ensure type compatibility between Guid and string
        var affectedObjects = _session.Query<ObjectProperties>()
            .Where(op => op.AlbumIds.Any(albumId => request.ObjectPropertiesIds.Contains(albumId.ToString())))
            .ToAsyncEnumerable(cancellationToken);

        await foreach (var obj in affectedObjects)
        {
            obj.AlbumIds.RemoveAll(albumId => request.ObjectPropertiesIds.Contains(albumId.ToString()));
            _session.Store(obj);
        }

        await _session.SaveChangesAsync(cancellationToken);

        return new DeleteObjectPropertiesToAlbumResponse();
    }
}
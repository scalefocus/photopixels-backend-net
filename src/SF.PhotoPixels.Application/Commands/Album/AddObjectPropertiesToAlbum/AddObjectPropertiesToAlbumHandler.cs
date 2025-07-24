using Marten;
using Mediator;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.Album.AddObjectPropertiesToAlbum;

public class AddObjectPropertiesToAlbumHandler : IRequestHandler<AddObjectPropertiesToAlbumRequest, AddObjectPropertiesToAlbumResponses>
{
    private readonly IDocumentSession _session;

    public AddObjectPropertiesToAlbumHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async ValueTask<AddObjectPropertiesToAlbumResponses> Handle(AddObjectPropertiesToAlbumRequest request, CancellationToken cancellationToken)
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

        // Album - Add only unique IDs that are not already present
        foreach (var id in request.ObjectPropertiesIds)
        {
            if (!album.ObjectPropertiesIds.Contains(id))
                album.ObjectPropertiesIds.Add(id);
        }

        _session.Store(album);

        // ObjectProperties - Add to the album
        var affectedObjects = _session.Query<ObjectProperties>()
            .Where(op => request.ObjectPropertiesIds.Contains(op.Id))
            .ToAsyncEnumerable(cancellationToken);

        await foreach (var obj in affectedObjects)
        {
            if (!obj.AlbumIds.Contains(album.Id))
                obj.AlbumIds.Add(album.Id);

            _session.Store(obj);
        }

        await _session.SaveChangesAsync(cancellationToken);

        return new AddObjectPropertiesToAlbumResponse();
    }
}
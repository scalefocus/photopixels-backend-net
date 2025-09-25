using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class AddToAlbumHandler : IRequestHandler<AddToAlbumRequest, OneOf<AddToAlbumResponse, NotFound>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;
    private readonly IObjectStorage _objectStorage;

    public AddToAlbumHandler(IExecutionContextAccessor executionContextAccessor, IDocumentSession session, IObjectStorage objectStorage)
    {
        _executionContextAccessor = executionContextAccessor;
        _session = session;
        _objectStorage = objectStorage;
    }

    public async ValueTask<OneOf<AddToAlbumResponse, NotFound>> Handle(AddToAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ObjectId) || string.IsNullOrEmpty(request.AlbumId))
            return new NotFound();

        var album = await _session.LoadAsync<Domain.Entities.Album>(request.AlbumId, cancellationToken);

        if (album == null || album.UserId != _executionContextAccessor.UserId)
            return new NotFound();

        var objectProperties = await _session.LoadAsync<ObjectProperties>(request.ObjectId, cancellationToken);

        if (objectProperties == null || objectProperties.UserId != _executionContextAccessor.UserId)
            return new NotFound();

        var objectAlbum = new AlbumObject(request.ObjectId, request.AlbumId);

        _session.Store(objectAlbum);
        await _session.SaveChangesAsync(cancellationToken);


        return new AddToAlbumResponse { AlbumId = request.AlbumId, ObjectId = request.ObjectId };
    }
}
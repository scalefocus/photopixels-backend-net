using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;

public class TrashObjectsHandler : IRequestHandler<TrashObjectsRequest, ObjectVersioningResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectRepository _objectRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IDocumentSession _session;

    public TrashObjectsHandler(IExecutionContextAccessor executionContextAccessor,
        IObjectRepository objectRepository, IDocumentSession session, IAlbumRepository albumRepository)
    {
        _executionContextAccessor = executionContextAccessor;
        _objectRepository = objectRepository;
        _albumRepository = albumRepository;
        _session = session;
    }

    public async ValueTask<ObjectVersioningResponse> Handle(TrashObjectsRequest request, CancellationToken cancellationToken)
    {
        var objectsMetadata = await _session.Query<ObjectProperties>()
            .Where(objects => request.ObjectIds.Contains(objects.Id)).ToListAsync(cancellationToken);

        if (!objectsMetadata.Any())
        {
            return new NotFound();
        }

        await SendObjectToAlbumDeletedEvent(request, cancellationToken);

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId, cancellationToken);
        if (user == null)
        {
            return new NotFound();
        }

        _session.DeleteObjects(objectsMetadata);
        await _session.SaveChangesAsync(cancellationToken);

        long revision = 0;
        foreach (var objectMetadata in objectsMetadata)
        {
            revision = await _objectRepository.AddEvent(
                _executionContextAccessor.UserId,
                new MediaObjectTrashed(objectMetadata.Id, DateTimeOffset.UtcNow),
                cancellationToken
            );
        }

        return new VersioningResponse
        {
            Revision = revision
        };
    }

    private async ValueTask SendObjectToAlbumDeletedEvent(TrashObjectsRequest request, CancellationToken cancellationToken)
    {
        var albumsContainingObject = await _session.Query<AlbumObject>()
            .Where(ao => request.ObjectIds.Contains(ao.ObjectId)).ToListAsync(cancellationToken);

        foreach (var albumObject in  albumsContainingObject)
        {
            var evt = new ObjectToAlbumDeleted()
            {
                AlbumId = Guid.Parse(albumObject.AlbumId),
                ObjectId = albumObject.ObjectId,
                RemovedAt = DateTimeOffset.Now
            };

            await _albumRepository.AddAlbumEvent(evt.AlbumId, evt, cancellationToken);
        }
    }
}

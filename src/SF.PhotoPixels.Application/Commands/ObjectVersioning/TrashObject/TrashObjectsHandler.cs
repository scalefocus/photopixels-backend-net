using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;

public class TrashObjectsHandler : IRequestHandler<TrashObjectsRequest, ObjectVersioningResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectRepository _objectRepository;
    private readonly IDocumentSession _session;

    public TrashObjectsHandler(IExecutionContextAccessor executionContextAccessor, IObjectRepository objectRepository, IDocumentSession session)
    {
        _executionContextAccessor = executionContextAccessor;
        _objectRepository = objectRepository;
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

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId);
        if (user == null)
        {
            return new NotFound();
        }

        _session.DeleteObjects(objectsMetadata);
        await _session.SaveChangesAsync();

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
}
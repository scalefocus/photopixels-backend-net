using Marten;
using Marten.Linq.SoftDeletes;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;


namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;

public class RemoveFromTrashObjectsHandler : IRequestHandler<RemoveFromTrashObjectsRequest, ObjectVersioningResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectRepository _objectRepository;
    private readonly IDocumentSession _session;

    public RemoveFromTrashObjectsHandler(IExecutionContextAccessor executionContextAccessor, IObjectRepository objectRepository, IDocumentSession session)
    {
        _executionContextAccessor = executionContextAccessor;
        _objectRepository = objectRepository;
        _session = session;
    }

    public async ValueTask<ObjectVersioningResponse> Handle(RemoveFromTrashObjectsRequest request, CancellationToken cancellationToken)
    {
        var objectIds = await _session.Query<ObjectProperties>()
            .Where(obj => request.ObjectIds.Contains(obj.Id) && obj.MaybeDeleted())
            .Select(obj => obj.Id)
            .ToListAsync();

        if (!objectIds.Any())
        {
            return new NotFound();
        }

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId);
        if (user == null)
        {
            return new NotFound();
        }

        long revision = 0;
        foreach (var id in objectIds)
        {
            revision = await _objectRepository.AddEvent(
                _executionContextAccessor.UserId,
                new MediaObjectRemovedFromTrash(id, DateTimeOffset.UtcNow),
                cancellationToken
            );
        }

        return new VersioningResponse
        {
            Revision = revision
        };
    }
}
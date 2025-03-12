using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;


namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;

public class RemoveFromTrashObjectHandler : IRequestHandler<RemoveFromTrashObjectRequest, ObjectVersioningResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectRepository _objectRepository;
    private readonly IDocumentSession _session;

    public RemoveFromTrashObjectHandler(IExecutionContextAccessor executionContextAccessor, IObjectRepository objectRepository, IDocumentSession session)
    {
        _executionContextAccessor = executionContextAccessor;
        _objectRepository = objectRepository;
        _session = session;
    }

    public async ValueTask<ObjectVersioningResponse> Handle(RemoveFromTrashObjectRequest request, CancellationToken cancellationToken)
    {
        var objectMetadata = await _session.Query<ObjectProperties>()
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (objectMetadata == null)
        {
            return new NotFound();
        }

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId);

        if (user == null)
        {
            return new NotFound();
        }
        
        objectMetadata.TrashDate = null;
        _session.Update(objectMetadata);

        var revision = await _objectRepository.AddEvent(_executionContextAccessor.UserId, new MediaObjectRemovedFromTrash(request.Id), cancellationToken);

        return new VersioningResponse
        {
            Revision = revision,
        };
    }
}
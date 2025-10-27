using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;

public class TrashObjectHandler : IRequestHandler<TrashObjectRequest, ObjectVersioningResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectRepository _objectRepository;
    private readonly IDocumentSession _session;

    public TrashObjectHandler(IExecutionContextAccessor executionContextAccessor, IObjectRepository objectRepository, IDocumentSession session)
    {
        _executionContextAccessor = executionContextAccessor;
        _objectRepository = objectRepository;
        _session = session;
    }

    public async ValueTask<ObjectVersioningResponse> Handle(TrashObjectRequest request, CancellationToken cancellationToken)
    {
        var objectMetadata = await _session.Query<ObjectProperties>()
            .SingleOrDefaultAsync(x => x.Id == request.ObjectId, cancellationToken);

        if (objectMetadata == null)
        {
            return new NotFound();
        }

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId, cancellationToken);

        if (user == null)
        {
            return new NotFound();
        }

        _session.DeleteWhere<ObjectProperties>(op => op.Id == objectMetadata.Id);
        await _session.SaveChangesAsync(cancellationToken);

        var revision = await _objectRepository.AddEvent(_executionContextAccessor.UserId, new MediaObjectTrashed(request.ObjectId, DateTimeOffset.UtcNow), cancellationToken);

        return new VersioningResponse
        {
            Revision = revision,
        };
    }
}

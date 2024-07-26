using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;

public class DeleteObjectHandler : IRequestHandler<DeleteObjectRequest, ObjectVersioningResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectRepository _objectRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;

    public DeleteObjectHandler(IExecutionContextAccessor executionContextAccessor, IObjectRepository objectRepository, IDocumentSession session, IObjectStorage objectStorage)
    {
        _executionContextAccessor = executionContextAccessor;
        _objectRepository = objectRepository;
        _session = session;
        _objectStorage = objectStorage;
    }

    public async ValueTask<ObjectVersioningResponse> Handle(DeleteObjectRequest request, CancellationToken cancellationToken)
    {
        var objectMetadata = await _session.Query<ObjectProperties>()
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (objectMetadata == null)
        {
            return new NotFound();
        }

        var user = _session.Load<Domain.Entities.User>(_executionContextAccessor.UserId);

        if (user == null)
        {
            return new NotFound();
        }
        _objectStorage.DeleteObject(_executionContextAccessor.UserId, objectMetadata.GetImageName());
        _objectStorage.DeleteThumbnail(_executionContextAccessor.UserId, objectMetadata.GetThumbnailName());

        user.DecreaseUsedQuota(objectMetadata.SizeInBytes);
        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        var revision = await _objectRepository.AddEvent(_executionContextAccessor.UserId, new MediaObjectDeleted(request.Id), cancellationToken);

        return new VersioningResponse
        {
            Revision = revision,
        };
    }
}
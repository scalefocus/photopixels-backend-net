using Marten;
using Marten.Linq.SoftDeletes;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure;
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
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.MaybeDeleted(), cancellationToken);

        if (objectMetadata == null)
        {
            return new NotFound();
        }

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId);

        if (user == null && objectMetadata.Deleted)
        {
            user = await _session.LoadAsync<Domain.Entities.User>(objectMetadata.UserId);
        }

        if (user == null)
        {
            return new NotFound();
        }

        var isPhotoDeleted = _objectStorage.DeleteObject(user.Id, objectMetadata.GetFileName());

        var thumbnailExtension = Constants.SupportedVideoFormats.Contains($".{objectMetadata.Extension}") ? "png" : "webp";
        var isThumbnailDeleted = _objectStorage.DeleteThumbnail(user.Id, objectMetadata.GetThumbnailName(thumbnailExtension));

        if (!isPhotoDeleted || !isThumbnailDeleted)
        {
            return new NotFound();
        }

        user.DecreaseUsedQuota(objectMetadata.SizeInBytes);

        _session.Update(user);
        _session.HardDelete(objectMetadata);

        await _session.SaveChangesAsync(cancellationToken);

        var revision = await _objectRepository.AddEvent(user.Id, new MediaObjectDeleted(request.Id), cancellationToken);

        return new VersioningResponse
        {
            Revision = revision,
        };
    }
}
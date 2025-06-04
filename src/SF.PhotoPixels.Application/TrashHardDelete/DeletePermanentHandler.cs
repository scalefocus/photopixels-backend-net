using Marten;
using Marten.Linq.SoftDeletes;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OneOf.Types;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure;
using SF.PhotoPixels.Infrastructure.Repositories;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.TrashHardDelete;

public class DeletePermanentHandler : IRequestHandler<DeletePermanentRequest, ObjectVersioningResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectStorage _objectStorage;
    private readonly IObjectRepository _objectRepository;
    private readonly IDocumentSession _session;
    private readonly ILogger<DeletePermanentHandler> _logger;

    public DeletePermanentHandler(
        IDocumentSession session,
        IExecutionContextAccessor executionContextAccessor,
        IObjectStorage objectStorage,
        IObjectRepository objectRepository,
        ILogger<DeletePermanentHandler> logger)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _objectStorage = objectStorage;
        _objectRepository = objectRepository;
        _logger = logger;
    }

    public async ValueTask<ObjectVersioningResponse> Handle(DeletePermanentRequest request, CancellationToken cancellationToken)
    {
        var objects = await _session.Query<ObjectProperties>()
            .Where(obj => request.ObjectIds.Contains(obj.Id) && obj.MaybeDeleted())
            .ToListAsync();

        if (!objects.Any())
        {
            return new NotFound();
        }

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId);
        if (user == null)
        {
            return new NotFound();
        }

#if DEBUG
        GC.Collect();
        GC.WaitForPendingFinalizers();
#endif

        long revision = 0;
        foreach (var obj in objects)
        {
            _ = _objectStorage.DeleteObject(user.Id, obj.GetFileName());

            var thumbnailExtension = Constants.SupportedVideoFormats.Contains($".{obj.Extension}") ? "png" : "webp";
            _ = _objectStorage.DeleteThumbnail(user.Id, obj.GetThumbnailName(thumbnailExtension));

            user.DecreaseUsedQuota(obj.SizeInBytes);

            revision = await _objectRepository.AddEvent(user.Id, new MediaObjectDeleted(obj.Id), cancellationToken);
        }

        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        return new VersioningResponse
        {
            Revision = revision
        };
    }
}
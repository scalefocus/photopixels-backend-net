using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.AlbumObjects;

public class DeleteAlbumObjectHandler : IRequestHandler<DeleteAlbumObjectRequest, OneOf<Success, ValidationError>>
{
    private readonly IAlbumRepository _albumRepository;

    public DeleteAlbumObjectHandler(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(DeleteAlbumObjectRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AlbumId))
        {
            return new ValidationError("IllegalUserInput", "Album cannot be null or empty");
        }

        if (!Guid.TryParse(request.AlbumId, out var albumGuid))
        {
            return new ValidationError("IllegalUserInput", "AlbumId should be guid");
        }

        if (request.ObjectIds.Length == 0)
        {
            return new ValidationError("IllegalUserInput", "Add objects to be removed from the album");
        }

        foreach (var objectId in request.ObjectIds)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                return new ValidationError("IllegalUserInput", "ObjectId cannot be null or empty");
            }

            var evt = new ObjectToAlbumDeleted
            {
                AlbumId = albumGuid,
                ObjectId = objectId,
                RemovedAt = DateTimeOffset.Now
            };

            await _albumRepository.AddAlbumEvent(evt.AlbumId, evt, cancellationToken);
        }

        return new Success();
    }
}

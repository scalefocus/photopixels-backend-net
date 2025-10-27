using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.Albums;

public class DeleteAlbumHandler : IRequestHandler<DeleteAlbumRequest, OneOf<Success, ValidationError>>
{
    private readonly IDocumentSession _session;
    private readonly IAlbumRepository _albumRepository;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public DeleteAlbumHandler(IDocumentSession session, IAlbumRepository albumRepository,
        IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _albumRepository = albumRepository;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(DeleteAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AlbumId))
        {
            return new ValidationError("IllegalUserInput", "Album cannot be null or empty");
        }

        if(!Guid.TryParse(request.AlbumId, out var albumGuid))
        {
            return new ValidationError("IllegalUserInput", "AlbumId should be guid");
        }

        var album = await _session.LoadAsync<Album>(request.AlbumId, cancellationToken);

        if (album == null) return new ValidationError("AlbumNotFound","Album cannot be loaded");

        var evt = new AlbumDeleted
        {
            UserId = _executionContextAccessor.UserId,
            AlbumId = albumGuid,
            Timestamp = DateTimeOffset.Now
        };

        await _albumRepository.AddAlbumEvent(evt.UserId, evt, cancellationToken);

        return new Success();
    }
}

using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Commands.Albums;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

public class UpdateAlbumHandler : IRequestHandler<UpdateAlbumRequest, OneOf<Success, ValidationError>>
{
    private readonly IDocumentSession _session;
    private readonly IAlbumRepository _albumRepository;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public UpdateAlbumHandler(IDocumentSession session, IAlbumRepository albumRepository,
        IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _albumRepository = albumRepository;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(UpdateAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            return new ValidationError("IllegalUserInput", "Id cannot be null or empty");
        }

        if (string.IsNullOrEmpty(request.Name))
        {
            return new ValidationError("IllegalUserInput", "Name cannot be null or empty");
        }

        if (!Guid.TryParse(request.Id, out var albumGuid))
        {
            return new ValidationError("IllegalUserInput", "AlbumId shoud be guid");
        }

        var album = await _session.LoadAsync<Album>(request.Id, cancellationToken);

        if (album == null) return new ValidationError("AlbumNotFound","Album cannot be loaded");

        var evt = new AlbumUpdated
        {
            AlbumId = albumGuid,
            UserId = _executionContextAccessor.UserId,
            Name = request.Name,
            IsSystem = request.IsSystem,
        };

        await _albumRepository.AddAlbumEvent(evt.UserId, evt, cancellationToken);
        return new Success();
    }
}

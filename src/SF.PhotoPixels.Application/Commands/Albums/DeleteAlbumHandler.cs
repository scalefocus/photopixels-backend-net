using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.Albums;

public class DeleteAlbumHandler : IRequestHandler<DeleteAlbumRequest, OneOf<Success, ValidationError>>
{    
    private readonly IDocumentSession _session;
    private readonly IAlbumRepository _albumRepository;

    public DeleteAlbumHandler(IDocumentSession session, IAlbumRepository albumRepository)
    {        
        _session = session;
        _albumRepository = albumRepository;
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(DeleteAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AlbumId))
        {
            return new ValidationError("IllegalUserInput", "Album cannot be null or empty");
        }

        if(!Guid.TryParse(request.AlbumId, out var albumGuid))
        {
            return new ValidationError("IllegalUserInput", "AlbumId shoud be guid");
        }

        var album = await _session.LoadAsync<Album>(request.AlbumId, cancellationToken);
        if (album != null)
        {
            var evt = new AlbumDeleted(albumGuid);

            await _albumRepository.AddAlbumEvent(albumGuid, evt, cancellationToken);            
        }

        return new Success();
    }    
}
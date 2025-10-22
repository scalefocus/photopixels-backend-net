using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Commands.Albums;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

public class UpdateAlbumHandler : IRequestHandler<UpdateAlbumRequest, OneOf<Success, ValidationError>>
{    
    private readonly IDocumentSession _session;    
    private readonly IAlbumRepository _albumRepository;

    public UpdateAlbumHandler(IDocumentSession session, IAlbumRepository albumRepository)
    {        
        _session = session;        
        _albumRepository = albumRepository;
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
        if (album != null)
        {
            var evt = new AlbumUpdated
            {  
                AlbumId = albumGuid,
                Name = request.Name,
                IsSystem = request.IsSystem,                
            };

            await _albumRepository.AddAlbumEvent(albumGuid, evt, cancellationToken);
        }                
        return new Success();
    }
}
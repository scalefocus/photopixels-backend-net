using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.Albums;

public class DeleteAlbumHandler : IRequestHandler<DeleteAlbumRequest, OneOf<Success, ValidationError>>
{    
    private readonly IDocumentSession _session;
    
    public DeleteAlbumHandler(IDocumentSession session)
    {        
        _session = session;    
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(DeleteAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AlbumId))
        {
            return new ValidationError("IllegalUserInput", "Album cannot be null or empty");
        }

        var album = await _session.LoadAsync<Album>(request.AlbumId, cancellationToken);
        if (album != null)
        {
            _session.Delete(album);

            var albumObjects = _session.Query<AlbumObject>().Where(x => x.AlbumId == request.AlbumId);
            _session.DeleteWhere<AlbumObject>(x => x.AlbumId == request.AlbumId);

            await _session.SaveChangesAsync(cancellationToken);
        }
                                
        return new Success();
    }    
}
using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.AlbumObjects;

public class DeleteAlbumObjectHandler : IRequestHandler<DeleteAlbumObjectRequest, OneOf<Success, ValidationError>>
{    
    private readonly IDocumentSession _session;    

    public DeleteAlbumObjectHandler(IDocumentSession session)
    { 
        _session = session;     
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(DeleteAlbumObjectRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AlbumId))
        {
            return new ValidationError("IllegalUserInput", "Album cannot be null or empty");
        }

        _session.DeleteWhere<AlbumObject>(x => x.AlbumId == request.AlbumId && request.ObjectIds.Contains(x.ObjectId));
        
        await _session.SaveChangesAsync(cancellationToken);
        
        return new Success();
    }    
}
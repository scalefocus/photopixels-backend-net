using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Commands.Albums;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;

public class UpdateAlbumHandler : IRequestHandler<UpdateAlbumRequest, OneOf<Success, ValidationError>>
{    
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public UpdateAlbumHandler(IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {        
        _session = session;
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

        var album = await _session.LoadAsync<Album>(request.Id, cancellationToken);
        if (album != null)
        {
            album.Name = request.Name;
                       
            _session.Update(album);
            await _session.SaveChangesAsync(cancellationToken);            
        }                
        return new Success();
    }
}
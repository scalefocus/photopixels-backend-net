using Marten;
using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Commands.Albums;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;

public class DeleteAlbumHandler : IRequestHandler<DeleteAlbumRequest, OneOf<Success, ValidationError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public DeleteAlbumHandler(UserManager<Domain.Entities.User> userManager, IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _userManager = userManager;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
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
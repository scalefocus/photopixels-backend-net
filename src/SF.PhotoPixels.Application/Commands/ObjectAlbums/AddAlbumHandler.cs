using Marten;

using Mediator;

using Microsoft.AspNetCore.Identity;

using OneOf;
using OneOf.Types;

using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands;

public class AddObjectToAlbumHandler : IRequestHandler<AddObjectToAlbumRequest, OneOf<Success, ValidationError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public AddObjectToAlbumHandler(UserManager<Domain.Entities.User> userManager, IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _userManager = userManager;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(AddObjectToAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AlbumId))
        {
            return new ValidationError("IllegalUserInput", "Album cannot be null or empty");
        }

        if (string.IsNullOrEmpty(request.ObjectId))
        {
            return new ValidationError("IllegalUserInput", "Object cannot be null or empty");
        }

        var objectAlbum = new ObjectAlbum(request.ObjectId, request.AlbumId);
        
        _session.Store(objectAlbum);
        await _session.SaveChangesAsync();

        return new Success();
    }
}

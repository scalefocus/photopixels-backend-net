using Marten;

using Mediator;

using Microsoft.AspNetCore.Identity;

using OneOf;
using OneOf.Types;

using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.AlbumObjects;

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

        if (request.ObjectIds == null || request.ObjectIds.Length == 0 )
        {
            return new ValidationError("IllegalUserInput", "Add objects to be atached to the album");
        }

        foreach (var objectId in request.ObjectIds)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                return new ValidationError("IllegalUserInput", "ObjectId cannot be null or empty");
            }

            var albumObject = new AlbumObject(request.AlbumId, objectId);            
            _session.Store(albumObject);
        }
        
        await _session.SaveChangesAsync();

        return new Success();
    }
}

using Marten;
using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;

namespace SF.PhotoPixels.Application.Commands.Albums;

public class AddAlbumHandler : IRequestHandler<AddAlbumRequest, OneOf<AddAlbumResponse, ValidationError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public AddAlbumHandler(UserManager<Domain.Entities.User> userManager, IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _userManager = userManager;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<AddAlbumResponse, ValidationError>> Handle(AddAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            return new ValidationError("IllegalUserInput", "Name cannot be null or empty");
        }

        var album = new Domain.Entities.Album
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            IsSystem = false,
            DateCreated = DateTimeOffset.UtcNow,
            UserId = _executionContextAccessor.UserId
        };
        _session.Store(album);
        await _session.SaveChangesAsync();

        return new AddAlbumResponse
        {
            Id = album.Id,
            Name = album.Name,
        };
    }
}

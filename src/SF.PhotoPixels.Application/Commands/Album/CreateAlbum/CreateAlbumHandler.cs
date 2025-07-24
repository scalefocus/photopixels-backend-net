using Marten;
using Mediator;
using SF.PhotoPixels.Application.Core;


namespace SF.PhotoPixels.Application.Commands.Album.CreateAlbum;

public class CreateAlbumHandler : IRequestHandler<CreateAlbumRequest, CreateAlbumResponses>
{
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public CreateAlbumHandler(IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<CreateAlbumResponses> Handle(CreateAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new ValidationError("Name", "Album name is required.");
        }

        // Check request.ObjectPropertiesIds if exist

        Domain.Entities.Album album = new Domain.Entities.Album 
        {
            Name = request.Name,
            ObjectPropertiesIds = request.ObjectPropertiesIds ?? new List<string>(),
            UserId = _executionContextAccessor.UserId
        };

        _session.Store(album);
        await _session.SaveChangesAsync(cancellationToken);

        return new CreateAlbumResponse { Album = album };
    }
}
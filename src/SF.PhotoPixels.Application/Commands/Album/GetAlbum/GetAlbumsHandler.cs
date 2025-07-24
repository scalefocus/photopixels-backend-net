using Marten;
using Mediator;
using SF.PhotoPixels.Application.Core;

namespace SF.PhotoPixels.Application.Commands.Album.GetAlbum;

public class GetAlbumsHandler : IRequestHandler<GetAlbumsRequest, GetAlbumsResponses>
{
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public GetAlbumsHandler(IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<GetAlbumsResponses> Handle(GetAlbumsRequest request, CancellationToken cancellationToken)
    {
        var albumIds = await _session.Query<Domain.Entities.Album>()
            .Where(album => album.UserId == _executionContextAccessor.UserId)
            .Select(album => album.Id)
            .ToListAsync(cancellationToken);

        if (albumIds is null || !albumIds.Any())
        {
            return new ValidationError("Id", "Albums not found.");
        }

        // Explicitly convert IReadOnlyList<string> to List<string>
        return new GetAlbumsResponse { Albums = albumIds.Select(x => x.ToString()) };
    }
}
using Marten;
using Mediator;
using Microsoft.Extensions.Logging;
using OneOf;
using SF.PhotoPixels.Application.Core;

namespace SF.PhotoPixels.Application.Query.Album;

public class GetAlbumsHandler : IQueryHandler<GetAlbumsRequest, OneOf<GetAlbumsResponse, ValidationError>>
{
    private readonly ILogger<GetAlbumsHandler> _logger;
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public GetAlbumsHandler(ILogger<GetAlbumsHandler> logger, IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _logger = logger;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<GetAlbumsResponse, ValidationError>> Handle(GetAlbumsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestType}", request.GetType().Name);
        var albums = await _session.Query<Domain.Entities.Album>()
            .Where(x => x.UserId == _executionContextAccessor.UserId)
            .ToListAsync(cancellationToken);
        // Handle the request and populate the response

        var albumResponses = new GetAlbumsResponse();
        albumResponses.Albums = albums.Select(album => new GetAlbumsResponse.AlbumResponse
        {
            Id = album.Id,
            Name = album.Name,
            IsSystem = album.IsSystem,
            DateCreated = album.DateCreated,

        }).ToList();

        return albumResponses;
    }
}


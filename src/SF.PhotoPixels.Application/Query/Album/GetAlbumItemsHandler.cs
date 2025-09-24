using Marten;

using Mediator;

using Microsoft.Extensions.Logging;

using OneOf;

using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Enums;
using SF.PhotoPixels.Infrastructure;

namespace SF.PhotoPixels.Application.Query.Albums;

public class GetAlbumItemsHandler : IQueryHandler<GetAlbumItemsRequest, OneOf<GetAlbumItemsResponse, ValidationError>>
{
    private readonly ILogger<GetAlbumItemsHandler> _logger;
    private readonly IDocumentSession _session;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public GetAlbumItemsHandler(ILogger<GetAlbumItemsHandler> logger, IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _logger = logger;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<GetAlbumItemsResponse, ValidationError>> Handle(GetAlbumItemsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestType}", request.GetType().Name);

        var itemsPerAlbumIds = await _session.Query<AlbumObject>()
            .Where(x => x.AlbumId == request.AlbumId)
            .Select(x => x.ObjectId)
            .ToListAsync(cancellationToken);
        var itemsPerAlbum = await _session.Query<ObjectProperties>()
                .Where(x => itemsPerAlbumIds.Contains(x.Id) && x.UserId == _executionContextAccessor.UserId).ToListAsync(cancellationToken);

        var result = new GetAlbumItemsResponse();

        result.Properties = itemsPerAlbum.Select(objectProperty => new PropertiesResponse
        {
            Id = objectProperty.Id,
            DateCreated = objectProperty.DateCreated,
            MediaType = GetMediaType(objectProperty.Extension)
        }).ToList();

        return result;
    }

    public string? GetMediaType(string extension)
    {
        return extension switch
        {
            var ext when Constants.SupportedVideoFormats.Contains($".{ext}") => MediaType.Video.ToString().ToLower(),
            var ext when Constants.SupportedPhotoFormats.Contains($".{ext}") => MediaType.Photo.ToString().ToLower(),
            _ => MediaType.Unknown.ToString().ToLower()
        };
    }

}






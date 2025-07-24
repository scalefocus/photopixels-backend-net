using Marten;
using Mediator;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Enums;
using SF.PhotoPixels.Infrastructure;

namespace SF.PhotoPixels.Application.Commands.Album.GetAlbum;

public class GetAlbumHandler : IRequestHandler<GetAlbumRequest, GetAlbumResponses>
{
    private readonly IDocumentSession _session;

    public GetAlbumHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async ValueTask<GetAlbumResponses> Handle(GetAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = await _session.LoadAsync<SF.PhotoPixels.Domain.Entities.Album>(request.Id, cancellationToken);
        if (album is null)
        {
            return new ValidationError("Id", "Album not found.");
        }

        var pageSize = 10;
        var properties = new List<PropertiesResponse>();

        var objectProperties = await _session.LoadManyAsync<ObjectProperties>(album.ObjectPropertiesIds);
        foreach (var obj in objectProperties.Take(pageSize))
        {
            var thumbnailProperty = new PropertiesResponse
            {
                Id = obj.Id,
                DateCreated = obj.DateCreated,
                MediaType = GetMediaType(obj.Extension)
            };

            properties.Add(thumbnailProperty);
        }

        var lastId = objectProperties.Count < pageSize ? "" : objectProperties[^1].Id;
        return new GetAlbumResponse() { Properties = properties, LastId = lastId };
    }

    private string? GetMediaType(string extension)
    {
        return extension switch
        {
            var ext when Constants.SupportedVideoFormats.Contains($".{ext}") => MediaType.Video.ToString().ToLower(),
            var ext when Constants.SupportedPhotoFormats.Contains($".{ext}") => MediaType.Photo.ToString().ToLower(),
            _ => MediaType.Unknown.ToString().ToLower()
        };
    }

}

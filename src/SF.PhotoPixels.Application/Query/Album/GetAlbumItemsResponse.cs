using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;

namespace SF.PhotoPixels.Application.Query.Albums;

public class GetAlbumItemsResponse
{
    public List<PropertiesResponse> Properties { get; set; } = new();
    public string AlbumId { get; internal set; } = string.Empty;
}
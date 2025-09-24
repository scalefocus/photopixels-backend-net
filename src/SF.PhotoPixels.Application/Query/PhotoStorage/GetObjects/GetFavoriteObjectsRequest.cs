using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;

public class GetFavoriteObjectsRequest : IQuery<OneOf<GetObjectsResponse, None>>
{
    [FromQuery]
    public string? LastId { get; set; }
    [FromQuery]
    public required int PageSize { get; set; }
}
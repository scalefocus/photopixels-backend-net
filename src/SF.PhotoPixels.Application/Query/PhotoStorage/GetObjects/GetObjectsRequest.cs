using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;

public class GetObjectsRequest : IQuery<OneOf<GetObjectsResponse, None>>
{
    [FromQuery]
    public string? LastId { get; set; }
    [FromQuery]
    public required int PageSize { get; set; }
}
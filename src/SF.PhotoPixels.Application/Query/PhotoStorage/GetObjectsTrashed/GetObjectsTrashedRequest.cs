using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectsTrashed;

public class GetObjectsTrashedRequest : IQuery<OneOf<GetObjectsTrashedResponse, None>>
{
    [FromQuery]
    public string? LastId { get; set; }
    [FromQuery]
    public required int PageSize { get; set; }
}
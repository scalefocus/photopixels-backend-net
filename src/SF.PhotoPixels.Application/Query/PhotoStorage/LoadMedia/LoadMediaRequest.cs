using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class LoadMediaRequest : IQuery<QueryResponse<LoadMediaResponse>>
{
    public required string Id { get; set; }

    [FromQuery]
    public string? Format { get; set; }
}
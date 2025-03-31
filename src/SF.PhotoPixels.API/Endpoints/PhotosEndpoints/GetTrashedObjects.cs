using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectsTrashed;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;


public class GetObjectsTrashed : EndpointBaseAsync
    .WithRequest<GetObjectsTrashedRequest>
    .WithActionResult<GetObjectsTrashedResponse>
{
    private readonly IMediator _mediator;

    public GetObjectsTrashed(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/objects/trashed")]
    [SwaggerOperation(
        Summary = "Get page size of objects in the trashed state",
        Tags = new[] { "Object operations" })
    ]
    public override async Task<ActionResult<GetObjectsTrashedResponse>> HandleAsync([FromRoute] GetObjectsTrashedRequest objectIds, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(objectIds, cancellationToken);

        return result.Match<ActionResult<GetObjectsTrashedResponse>>(
                   response => Ok(response),
                   _ => new NoContentResult()
                   );
    }
}
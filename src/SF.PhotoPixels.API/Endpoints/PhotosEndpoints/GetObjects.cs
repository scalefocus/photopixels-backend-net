using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;


public class GetObjects : EndpointBaseAsync
    .WithRequest<GetObjectsRequest>
    .WithActionResult<GetObjectsResponse>
{
    private readonly IMediator _mediator;

    public GetObjects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/objects")]
    [SwaggerOperation(
        Summary = "Get page size of objects",
        Tags = new[] { "Object operations" })
    ]
    public override async Task<ActionResult<GetObjectsResponse>> HandleAsync([FromRoute] GetObjectsRequest objectIds, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(objectIds, cancellationToken);

        return result.Match<ActionResult<GetObjectsResponse>>(
                   response => Ok(response),
                   _ => new NoContentResult()
                   );
    }
}
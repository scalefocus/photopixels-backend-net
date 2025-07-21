using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class GetFavoriteObjects : EndpointBaseAsync
    .WithRequest<GetFavoriteObjectsRequest>
    .WithActionResult<GetObjectsResponse>
{
    private readonly IMediator _mediator;

    public GetFavoriteObjects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/objects/favorites")]
    [SwaggerOperation(
        Summary = "Get favorite objects with page size of objects",
        Tags = new[] { "Object operations" })
    ]
    public override async Task<ActionResult<GetObjectsResponse>> HandleAsync([FromRoute] GetFavoriteObjectsRequest objectIds, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(objectIds, cancellationToken);

        return result.Match<ActionResult<GetObjectsResponse>>(
                   response => Ok(response),
                   _ => new NoContentResult()
                   );
    }
}
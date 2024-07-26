using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectData;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class GetObjectData : EndpointBaseAsync
    .WithRequest<GetObjectDataRequest>
    .WithActionResult<ObjectDataResponse>
{
    private readonly IMediator _mediator;

    public GetObjectData(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/object/{ObjectId}/data")]
    [SwaggerOperation(
        Summary = "Get the object properties and thumbnail",
        Description = "Returns the object properties and thumbnail",
        Tags = new[] { "Object operations" })
    ]
    public override async Task<ActionResult<ObjectDataResponse>> HandleAsync([FromRoute] GetObjectDataRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<ObjectDataResponse>>(
            x => new OkObjectResult(x),
            _ => new NotFoundResult()
        );
    }
}
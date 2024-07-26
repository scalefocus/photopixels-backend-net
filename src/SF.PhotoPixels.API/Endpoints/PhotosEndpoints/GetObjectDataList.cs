using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectData;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectDataList;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class GetObjectDataList : EndpointBaseAsync
    .WithRequest<GetObjectDataListRequest>
    .WithActionResult<IList<ObjectDataResponse>>
{
    private readonly IMediator _mediator;

    public GetObjectDataList(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/objects/data")]
    [SwaggerOperation(
        Summary = "Get the properties and thumbnail of objects",
        Description = "Returns the properties and thumbnail of objects",
        Tags = new[] { "Object operations" })
    ]
    public override async Task<ActionResult<IList<ObjectDataResponse>>> HandleAsync(GetObjectDataListRequest objectIds, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(objectIds, cancellationToken);

        return result.Match<ActionResult<IList<ObjectDataResponse>>>(
            response => new OkObjectResult(response),
            _ => new NotFoundResult(),
            e => new BadRequestObjectResult(e)
        );
    }
}
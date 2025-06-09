using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class TrashObjects : EndpointBaseAsync
    .WithRequest<TrashObjectsRequest>
    .WithActionResult<ObjectVersioningResponse>
{
    private readonly IMediator _mediator;

    public TrashObjects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object/trash")]
    [SwaggerOperation(
            Summary = "Trash objects",
            Description = "Trash objects to the server")
    ]
    public override async Task<ActionResult<ObjectVersioningResponse>> HandleAsync([FromBody] TrashObjectsRequest trashObjectsRequest, CancellationToken cancellationToken = default)
    {
       // var trashObjectsRequest = new TrashObjectsRequest { ObjectIds = new List<string>() };

        var result = await _mediator.Send(trashObjectsRequest, cancellationToken);

        return result.Match<ActionResult<ObjectVersioningResponse>>(
            response => new OkObjectResult(response),
            _ => new NotFoundResult()
        );
    }
}
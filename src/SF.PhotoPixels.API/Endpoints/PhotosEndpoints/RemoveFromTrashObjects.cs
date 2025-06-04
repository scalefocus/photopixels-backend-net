using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class RemoveFromTrashObjects : EndpointBaseAsync
    .WithRequest<RemoveFromTrashObjectsRequest>
    .WithActionResult<ObjectVersioningResponse>
{
    private readonly IMediator _mediator;

    public RemoveFromTrashObjects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object/trash/removeObjects")]
    [SwaggerOperation(
            Summary = "Remove items from trash",
            Description = "Remove items from trash on the server.",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<ObjectVersioningResponse>> HandleAsync([FromBody] RemoveFromTrashObjectsRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<ObjectVersioningResponse>>(
            response => new OkObjectResult(response),
            _ => new NotFoundResult()
        );
    }
}

using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class RemoveFromTrash : EndpointBaseAsync
    .WithRequest<RemoveFromTrashObjectRequest>
    .WithActionResult<ObjectVersioningResponse>
{
    private readonly IMediator _mediator;

    public RemoveFromTrash(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object/trash/remove")]
    [SwaggerOperation(
            Summary = "Remove item from trash",
            Description = "Remove item from trash on the server.",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<ObjectVersioningResponse>> HandleAsync([FromBody] RemoveFromTrashObjectRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<ObjectVersioningResponse>>(
            response => new OkObjectResult(response),
            e => new BadRequestObjectResult(e)
        );
    }
}
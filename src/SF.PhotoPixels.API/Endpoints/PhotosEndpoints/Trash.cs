using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class Trash : EndpointBaseAsync
    .WithRequest<TrashObjectRequest>
    .WithActionResult<ObjectVersioningResponse>
{
    private readonly IMediator _mediator;

    public Trash(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("/object/{objectid}/trash")]
    [SwaggerOperation(
            Summary = "Trash a photo",
            Description = "Trash a photo to the server",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<ObjectVersioningResponse>> HandleAsync( TrashObjectRequest trashObjectRequest, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send( trashObjectRequest, cancellationToken);

        return result.Match<ActionResult<ObjectVersioningResponse>>(
            response => new OkObjectResult(response),
            e => new BadRequestObjectResult(e)
        );
    }
}
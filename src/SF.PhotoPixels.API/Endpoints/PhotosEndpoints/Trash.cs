using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.UpdateObject;
using SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class Trash : EndpointBaseAsync
    .WithRequest<TrashObjectRequest>
    .WithActionResult<StorePhotoResponse>
{
    private readonly IMediator _mediator;

    public Trash(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object/trash")]
    [SwaggerOperation(
            Summary = "Trash a photo",
            Description = "Trash a photo to the server",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<StorePhotoResponse>> HandleAsync([FromBody] TrashObjectRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<StorePhotoResponse>>(
            response => new OkObjectResult(response),
            e => new BadRequestObjectResult(e)
        );
    }
}
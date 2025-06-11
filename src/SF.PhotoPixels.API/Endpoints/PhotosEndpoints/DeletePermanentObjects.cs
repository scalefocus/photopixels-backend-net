using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using SF.PhotoPixels.Application.TrashHardDelete;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class DeletePermanentObjects : EndpointBaseAsync
    .WithRequest<DeletePermanentRequest>
    .WithActionResult<ObjectVersioningResponse>
{
    private readonly IMediator _mediator;

    public DeletePermanentObjects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object/trash/deletePermanent")]
    [SwaggerOperation(
            Summary = "Delete permanently objects from trash",
            Description = "\"Delete permanently objects from trash on the server.",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<ObjectVersioningResponse>> HandleAsync([FromBody] DeletePermanentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<ObjectVersioningResponse>>(
            response => new OkObjectResult(response),
            _ => new NotFoundResult()
        );
    }
}
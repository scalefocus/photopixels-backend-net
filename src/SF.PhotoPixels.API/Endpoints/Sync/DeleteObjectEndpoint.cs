using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Sync;

public class DeleteObjectEndpoint : EndpointBaseAsync.WithRequest<DeleteObjectRequest>.WithActionResult<VersioningResponse>
{
    private readonly IMediator _mediator;

    public DeleteObjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("/object/{Id}")]
    [SwaggerOperation(
            Summary = "Delete an object",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<VersioningResponse>> HandleAsync([FromRoute] DeleteObjectRequest request, CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<VersioningResponse>>(
            response => Ok(response),
            _ => NotFound()
        );
    }
}
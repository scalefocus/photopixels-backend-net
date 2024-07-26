using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;
using Swashbuckle.AspNetCore.Annotations;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.UpdateObject;

namespace SF.PhotoPixels.API.Endpoints.Sync;
public class UpdateObjectEndpoint : EndpointBaseAsync.WithRequest<UpdateObjectRequest>.WithActionResult<VersioningResponse>
{
    private readonly IMediator _mediator;

    public UpdateObjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("/object/{Id}")]
    [SwaggerOperation(
            Summary = "Update an object",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<VersioningResponse>> HandleAsync([FromRoute] UpdateObjectRequest request, CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<VersioningResponse>>(
            response => Ok(response),
            _ => NotFound()
        );
    }
}
using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.StateChanges;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Sync;

public class GetChangesEndpoint : EndpointBaseAsync.WithRequest<StateChangesRequest>.WithActionResult<StateChangesResponseDetails>
{
    private readonly IMediator _mediator;

    public GetChangesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/revision/{RevisionId:long}")]
    [SwaggerOperation(
            Summary = "Get changes",
            Tags = new[] { "Sync" }),
    ]
    public override async Task<ActionResult<StateChangesResponseDetails>> HandleAsync([FromRoute] StateChangesRequest request, CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<StateChangesResponseDetails>>(
            response => Ok(response),
            _ => NotFound()
        );
    }
}
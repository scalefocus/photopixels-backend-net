using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.StateChanges;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Sync;

public class GetAlbumChangesEndpoint : EndpointBaseAsync.WithRequest<AlbumStateChangesRequest>.WithActionResult<AlbumStateChangesResponseDetails>
{
    private readonly IMediator _mediator;

    public GetAlbumChangesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("album/{AlbumId}/revision/{RevisionId:long}")]
    [SwaggerOperation(
            Summary = "Get album changes",
            Tags = ["Sync"]),
    ]
    public override async Task<ActionResult<AlbumStateChangesResponseDetails>> HandleAsync([FromRoute] AlbumStateChangesRequest request, CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<AlbumStateChangesResponseDetails>>(
            response => Ok(response),
            _ => NotFound()
        );
    }
}

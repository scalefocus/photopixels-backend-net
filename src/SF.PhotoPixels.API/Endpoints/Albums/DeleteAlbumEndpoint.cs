using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Album.DeleteAlbum;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Albums;

public class DeleteAlbumEndpoint : EndpointBaseAsync.WithRequest<Guid>.WithActionResult
{
    private readonly IMediator _mediator;

    public DeleteAlbumEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    //[AllowAnonymous]

    [HttpDelete("api/albums/{id:guid}")]
    [SwaggerOperation(
        Summary = "Delete an album",
        Tags = new[] { "Albums" }
    )]
    public override async Task<ActionResult> HandleAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteAlbumRequest { Id = id }, cancellationToken);

        //if (result) return NotFound();
        return NoContent();
    }
}
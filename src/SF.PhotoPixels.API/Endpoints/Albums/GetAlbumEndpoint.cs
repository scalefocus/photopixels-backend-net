using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Album.GetAlbum;
using SF.PhotoPixels.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Albums;

public class GetAlbumEndpoint : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<Album>
{
    private readonly IMediator _mediator;

    public GetAlbumEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    //[AllowAnonymous]

    [HttpGet("api/albums/{id:guid}")]
    [SwaggerOperation(
        Summary = "Get album by id",
        Tags = new[] { "Albums" }
    )]
    public override async Task<ActionResult<Album>> HandleAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAlbumRequest { Id = id }, cancellationToken);

        if (result.IsT1) return BadRequest(result.AsT1.Errors.First().Value);
        return Ok(result.AsT0);
    }
}

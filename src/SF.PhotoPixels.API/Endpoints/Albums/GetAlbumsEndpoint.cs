using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Album.GetAlbum;
using SF.PhotoPixels.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Albums;

public class GetAlbumsEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<List<Album>>
{
    private readonly IMediator _mediator;

    public GetAlbumsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    //[AllowAnonymous]

    [HttpGet("api/albums")]
    [SwaggerOperation(
        Summary = "Get albums",
        Tags = new[] { "Albums" }
    )]
    public override async Task<ActionResult<List<Album>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAlbumsRequest(), cancellationToken);

        if (result.IsT1) return BadRequest(result.AsT1.Errors.First().Value);
        return Ok(result.AsT0);
    }
}
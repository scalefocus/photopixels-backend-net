using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Album.CreateAlbum;
using SF.PhotoPixels.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Albums;

public class CreateAlbumEndpoint : EndpointBaseAsync.WithRequest<CreateAlbumRequest>.WithActionResult<Album>
{
    private readonly IMediator _mediator;

    public CreateAlbumEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    //[AllowAnonymous]

    [HttpPost("api/albums")]
    [SwaggerOperation(
        Summary = "Create a new album",
        Tags = new[] { "Albums" }
    )]
    public override async Task<ActionResult<Album>> HandleAsync([FromBody] CreateAlbumRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        if (result.IsT1) return BadRequest(result.AsT1.Errors.First().Value);
        return Ok(result.AsT0);
    }
}
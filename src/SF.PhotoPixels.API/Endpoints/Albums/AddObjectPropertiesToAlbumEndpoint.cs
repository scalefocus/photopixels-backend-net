using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SF.PhotoPixels.Application.Commands.Album.AddObjectPropertiesToAlbum;
using SF.PhotoPixels.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SF.PhotoPixels.API.Endpoints.Albums;

public class AddObjectPropertiesToAlbumEndpoint : EndpointBaseAsync.WithRequest<AddObjectPropertiesToAlbumRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public AddObjectPropertiesToAlbumEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    //[AllowAnonymous]
    [HttpPost("api/albums/objectproperties/add")]
    [SwaggerOperation(
        Summary = "Add a list of object property IDs to an album",
        Tags = new[] { "Albums" }
    )]
    public override async Task<ActionResult> HandleAsync(AddObjectPropertiesToAlbumRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        //if (result.IsT1) return BadRequest(result.AsT1.Errors.First().Value);
        return NoContent();
    }
}
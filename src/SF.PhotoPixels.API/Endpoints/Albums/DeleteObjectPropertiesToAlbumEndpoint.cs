using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Album.AddObjectPropertiesToAlbum;
using SF.PhotoPixels.Application.Commands.Album.DeleteObjectPropertiesToAlbum;
using SF.PhotoPixels.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.Albums;

public class DeleteObjectPropertiesToAlbumEndpoint : EndpointBaseAsync.WithRequest<DeleteObjectPropertiesToAlbumRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public DeleteObjectPropertiesToAlbumEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    //[AllowAnonymous]

    [HttpPost("api/albums/objectproperties/delete")]
    [SwaggerOperation(
        Summary = "Add a list of object property IDs to an album",
        Tags = new[] { "Albums" }
    )]
    public override async Task<ActionResult> HandleAsync(DeleteObjectPropertiesToAlbumRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        //if (result.IsT1) return BadRequest(result.AsT1.Errors.First().Value);
        return NoContent();
    }
}
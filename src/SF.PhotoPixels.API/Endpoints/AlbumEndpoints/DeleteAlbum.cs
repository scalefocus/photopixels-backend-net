using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Commands.Albums;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints;

public class DeleteAlbum : EndpointBaseAsync
    .WithRequest<DeleteAlbumRequest>
    .WithActionResult<OneOf<Success, ValidationError>>
{
    private readonly IMediator _mediator;

    public DeleteAlbum(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("/album/{albumId}/")]
    [SwaggerOperation(
            Summary = "Delete an album.",
            Description = "Delete an album, but keep the objects intact. Thy are just no longer part of this album",
            Tags = new[] { "Album operations" }),
    ]
    public override async Task<ActionResult<OneOf<Success, ValidationError>>> HandleAsync([FromRoute] DeleteAlbumRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<OneOf<Success, ValidationError>>>(
            response => new OkObjectResult(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}
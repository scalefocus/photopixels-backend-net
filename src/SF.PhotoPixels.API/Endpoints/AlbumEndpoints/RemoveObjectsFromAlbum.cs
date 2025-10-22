using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Commands.AlbumObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints;

public class RemoveObjectsFromAlbum : EndpointBaseAsync
    .WithRequest<DeleteAlbumObjectRequest>
    .WithActionResult<OneOf<Success, ValidationError>>
{
    private readonly IMediator _mediator;

    public RemoveObjectsFromAlbum(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("/albums/{albumId}/objects:bulk-delete")]
    [SwaggerOperation(
            Summary = "Remove object(s) from album",
            Description = "Remove object(s) from albumn. The actual objects remain intact. Just the album relation is removed.",
            Tags = new[] { "Album operations" }),
    ]
    public override async Task<ActionResult<OneOf<Success, ValidationError>>> HandleAsync(DeleteAlbumObjectRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<OneOf<Success, ValidationError>>>(
            response => new OkObjectResult(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}
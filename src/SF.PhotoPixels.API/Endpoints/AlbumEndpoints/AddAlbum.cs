using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Commands.Albums;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints;

public class AddAlbum : EndpointBaseAsync.WithRequest<AddAlbumRequest>.WithActionResult<OneOf<AddAlbumResponse, ValidationError>>
{
    private readonly IMediator _mediator;

    public AddAlbum(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/album/")]
    [SwaggerOperation(
            Summary = "Add a new album",
            Description = "Creates a new album and returns the new album info",
            OperationId = "Add_Album",
            Tags = new[] { "Album operations" }),
    ]
    public override async Task<ActionResult<OneOf<AddAlbumResponse, ValidationError>>> HandleAsync([FromBody] AddAlbumRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<OneOf<AddAlbumResponse, ValidationError>>>(
            response => new OkObjectResult(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}
using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Commands.Albums;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints;

public class UpdateAlbum : EndpointBaseAsync.WithRequest<UpdateAlbumRequest>.WithActionResult<OneOf<Success, ValidationError>>
{
    private readonly IMediator _mediator;

    public UpdateAlbum(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("/album/")]
    [SwaggerOperation(
            Summary = "Update an album",
            Description = "Updates an album",
            OperationId = "Update_Album",
            Tags = new[] { "Album operations" }),
    ]
    public override async Task<ActionResult<OneOf<Success, ValidationError>>> HandleAsync([FromBody] UpdateAlbumRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<OneOf<Success, ValidationError>>>(
            response => new OkObjectResult(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}
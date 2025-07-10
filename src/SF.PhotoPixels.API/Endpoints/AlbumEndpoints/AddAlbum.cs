using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class AddAlbum : EndpointBaseAsync.WithRequest<AddAlbumRequest>.WithActionResult<OneOf<Success, ValidationError>>
{
    private readonly IMediator _mediator;

    public AddAlbum(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/album/")]
    [SwaggerOperation(
            Summary = "Add a new album",
            Description = "Creates a new album",
            OperationId = "Add_Album",
            Tags = new[] { "Album operations" }),
    ]
    public override async Task<ActionResult<OneOf<Success, ValidationError>>> HandleAsync([FromBody] AddAlbumRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<OneOf<Success, ValidationError>>>(
            response => new OkObjectResult(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}
using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Query.Album;

using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints;

public class GetAlbums : EndpointBaseAsync.WithoutRequest.WithActionResult<OneOf<GetAlbumsResponse, ValidationError>>
{
    private readonly IMediator _mediator;

    public GetAlbums(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/album/")]
    [SwaggerOperation(
            Summary = "Get all albums",
            Description = "Retrieves a list of all albums",
            OperationId = "Get_Albums",
            Tags = new[] { "Album operations" }),
    ]
    public override async Task<ActionResult<OneOf<GetAlbumsResponse, ValidationError>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(GetAlbumsRequest.Instance, cancellationToken);

        return result.Match<ActionResult<OneOf<GetAlbumsResponse, ValidationError>>>(
            response => new OkObjectResult(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}
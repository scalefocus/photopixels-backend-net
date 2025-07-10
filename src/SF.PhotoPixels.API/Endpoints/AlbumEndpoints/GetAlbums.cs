using Ardalis.ApiEndpoints;
using Marten;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Query.Albums;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;
using SF.PhotoPixels.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

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
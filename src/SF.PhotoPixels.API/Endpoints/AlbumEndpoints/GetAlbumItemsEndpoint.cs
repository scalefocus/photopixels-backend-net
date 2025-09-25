using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Query.Album;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints;

public class GetAlbumItems : EndpointBaseAsync.WithRequest<GetAlbumItemsRequest>.WithActionResult<OneOf<GetAlbumItemsResponse, ValidationError>>
{
    private readonly IMediator _mediator;

    public GetAlbumItems(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/album/{albumId}/{lastId?}")]
    [SwaggerOperation(
            Summary = "Get all album items",
            Description = "Retrieves a list of all items in a specific album",
            OperationId = "Get_Album_Items",
            Tags = new[] { "Album operations" }),
    ]
    public override async Task<ActionResult<OneOf<GetAlbumItemsResponse, ValidationError>>> HandleAsync([FromRoute] GetAlbumItemsRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<OneOf<GetAlbumItemsResponse, ValidationError>>>(
            response => new OkObjectResult(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}
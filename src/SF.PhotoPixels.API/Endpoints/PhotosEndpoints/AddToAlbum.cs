using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class AddToAlbum : EndpointBaseAsync.WithRequest<AddToAlbumRequest>.WithActionResult<OneOf<AddToAlbumResponse, NotFound>>
{
    private readonly IMediator _mediator;

    public AddToAlbum(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("object/{objectId}/album/{albumId}")]
    [SwaggerOperation(
            Summary = "Add item to an album",
            Description = "Adds a photo/video to an album",
            OperationId = "Add_To_Album",
            Tags = new[] { "Album operations" }),
    ]
    public override async Task<ActionResult<OneOf<AddToAlbumResponse, NotFound>>> HandleAsync([FromRoute] AddToAlbumRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<OneOf<AddToAlbumResponse, NotFound>>>(
            response => new OkObjectResult(response),
            notFound => new NotFoundResult()
        );
    }
}
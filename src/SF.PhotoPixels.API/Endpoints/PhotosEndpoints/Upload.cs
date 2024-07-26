using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class Upload : EndpointBaseAsync.WithRequest<StorePhotoRequest>.WithActionResult<StorePhotoResponse>
{
    private readonly IMediator _mediator;

    public Upload(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object")]
    [SwaggerOperation(
            Summary = "Upload a photo",
            Description = "Uploads a photo to the server",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<StorePhotoResponse>> HandleAsync([FromForm] StorePhotoRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<StorePhotoResponse>>(
            response => new OkObjectResult(response),
            _ => new ConflictResult(),
            e => new BadRequestObjectResult(e)
        );
    }
}
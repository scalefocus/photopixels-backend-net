using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadPhoto;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class Download : EndpointBaseAsync.WithRequest<LoadPhotoRequest>.WithActionResult<FileStream>
{
    private readonly IMediator _mediator;

    public Download(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/object/{Id}")]
    [SwaggerOperation(
            Summary = "Download a photo",
            Description = "Downloads a photo from the server",
            OperationId = "Download_Photo",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<FileStream>> HandleAsync([FromRoute] LoadPhotoRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<FileStream>>(
            x => new FileStreamResult(x.Photo, x.ContentType),
            _ => new NotFoundResult()
        );
    }
}
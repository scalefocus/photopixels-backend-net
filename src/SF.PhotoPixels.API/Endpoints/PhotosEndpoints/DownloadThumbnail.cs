using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadThumbnail;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class DownloadThumbnail : EndpointBaseAsync
.WithRequest<LoadThumbnailRequest>
.WithActionResult<FileStream>
{
    private readonly IMediator _mediator;

    public DownloadThumbnail(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/object/{ObjectId}/thumbnail")]
    [SwaggerOperation(
        Summary = "Download a thumbnail of a object",
        Description = "Downloads a thumbnail of a object from the server",
        Tags = new[] { "Object operations" })
    ]
    public override async Task<ActionResult<FileStream>> HandleAsync([FromRoute] LoadThumbnailRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<FileStream>>(
            x => new FileStreamResult(x, "image/webp"),
            _ => new NotFoundResult()
        );
    }
}
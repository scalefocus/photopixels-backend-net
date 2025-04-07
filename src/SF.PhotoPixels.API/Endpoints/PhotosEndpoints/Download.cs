using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class Download : EndpointBaseAsync.WithRequest<LoadMediaRequest>.WithActionResult<FileStream>
{
    private readonly IMediator _mediator;

    public Download(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/object/{Id}")]
    [SwaggerOperation(
            Summary = "Download a photo/video",
            Description = "Downloads a photo/video from the server",
            OperationId = "Download_Media",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<FileStream>> HandleAsync([FromRoute] LoadMediaRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<FileStream>>(
            x => new FileStreamResult(x.Media, x.ContentType),
            _ => new NotFoundResult()
        );
    }
}
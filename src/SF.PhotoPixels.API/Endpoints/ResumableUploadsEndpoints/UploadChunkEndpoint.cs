using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Tus.SendChunk;
using SolidTUS.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.TusEndpoints;

public class UploadChunkEndpoint : EndpointBaseAsync.WithRequest<UploadChunkRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public UploadChunkEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [TusUpload("send_data/{fileId}")]
    [SwaggerOperation(
            Summary = "Continue resumable upload",
            Tags = ["Tus"]),
    ]
    public override async Task<ActionResult> HandleAsync([FromRoute] UploadChunkRequest request, CancellationToken cancellationToken = new())
    {
        await _mediator.Send(request, cancellationToken);

        return NoContent();
    }
}
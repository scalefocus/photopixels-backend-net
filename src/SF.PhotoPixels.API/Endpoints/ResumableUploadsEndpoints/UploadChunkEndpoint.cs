using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SolidTUS.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using SF.PhotoPixels.Application.Commands.Tus.SendChunk;

namespace SF.PhotoPixels.API.Endpoints.TusEndpoints;

public class UploadChunkEndpoint : EndpointBaseAsync.WithRequest<string>.WithActionResult
{
    private readonly IMediator _mediator;

    public UploadChunkEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [TusUpload("/send_data/{fileId}")]
    [SwaggerOperation(
            Summary = "Continue resumable upload",
            Tags = ["Tus"]),
    ]
    public override async Task<ActionResult> HandleAsync([FromRoute] string fileId, CancellationToken cancellationToken = new())
    {
        var request = new UploadChunkRequest()
        {
            FileId = fileId,
        };
        await _mediator.Send(request, cancellationToken);

        return NoContent();
    }
}
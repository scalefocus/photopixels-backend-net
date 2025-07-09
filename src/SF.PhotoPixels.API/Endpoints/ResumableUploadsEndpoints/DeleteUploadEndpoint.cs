using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Tus.DeleteUpload;
using SolidTUS.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.TusEndpoints;

public class DeleteUploadEndpoint : EndpointBaseAsync.WithRequest<string>.WithActionResult<DeleteUploadResponse>
{
    private readonly IMediator _mediator;

    public DeleteUploadEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [TusDelete("send_data/{fileId}")]
    [SwaggerOperation(
            Summary = "Delete ongoing resumable upload",
            Tags = ["Tus"]),
    ]
    public override async Task<ActionResult<DeleteUploadResponse>> HandleAsync([FromRoute] string fileId, CancellationToken cancellationToken = new())
    {
        var request = new DeleteUploadRequest()
        {
            FileId = fileId
        };

        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<DeleteUploadResponse>>(
            _ => NoContent(),
            _ => Forbid());
    }
}
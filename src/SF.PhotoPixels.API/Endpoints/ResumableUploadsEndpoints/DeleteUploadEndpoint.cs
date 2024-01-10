using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SolidTUS.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using SF.PhotoPixels.Application.Commands.Tus.DeleteUpload;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.API.Endpoints.TusEndpoints;

public class DeleteUploadEndpoint : EndpointBaseAsync.WithRequest<string>.WithActionResult<OneOf<NotFound, Success>>
{
    private readonly IMediator _mediator;

    public DeleteUploadEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [TusDelete("/send_data/{fileId}")]
    [SwaggerOperation(
            Summary = "Create resumable upload",
            Tags = new[] { "Tus" }),
    ]
    public override async Task<ActionResult<OneOf<NotFound, Success>>> HandleAsync([FromRoute] string fileId, CancellationToken cancellationToken = new())
    {
        var request = new DeleteUploadRequest()
        {
            FileId = fileId
        };

        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<OneOf<NotFound, Success>>>(
            response => NotFound(),
            _ => Ok()
        );
    }
}
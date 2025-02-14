using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SolidTUS.Attributes;
using SF.PhotoPixels.Application.Commands.Tus.CreateUpload;
using SF.PhotoPixels.Application.Commands.Tus;

namespace SF.PhotoPixels.API.Endpoints.TusEndpoints;

public class CreateUploadEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<CreateUploadResponse>
{
    private readonly IMediator _mediator;

    public CreateUploadEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [UpdateMetadata]
    [TusCreation("/create_upload")]
    [SwaggerOperation(
            Summary = "Create and get info on a new upload resource",
            Tags = ["Tus"]),
    ]
    public override async Task<ActionResult<CreateUploadResponse>> HandleAsync(CancellationToken cancellationToken = new())
    {
        var request = new CreateUploadRequest();
        var result = await _mediator.Send(request, cancellationToken);

        if (result.IsT1) return BadRequest(result.AsT1.Errors.First().Value);
        return Ok();
    }
}
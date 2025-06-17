using Ardalis.ApiEndpoints;
using JasperFx.Core;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Tus;
using SF.PhotoPixels.Application.Commands.Tus.CreateUpload;
using SolidTUS.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.TusEndpoints;

public class CreateUploadEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<CreateUploadResponse>
{
    private readonly IMediator _mediator;

    public CreateUploadEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [UpdateMetadata]
    [TusCreation("create_upload")]
    [SwaggerOperation(
            Summary = "Create and get info on a new upload resource",
            Tags = ["Tus"]),
    ]
    public override async Task<ActionResult<CreateUploadResponse>> HandleAsync(CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(CreateUploadRequest.Instance, cancellationToken);

        string location = Response.Headers["Location"];

        Response.Headers["Location"] = location.StartsWith("/")
            ? location.Substring(1)
            : location;

        if (result.IsT1) return BadRequest(result.AsT1.Errors.First().Value);
        return Ok();
    }
}
﻿using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class DownloadObjects : EndpointBaseAsync.WithRequest<DownloadObjectsRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public DownloadObjects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object/downloadZip")]
    [SwaggerOperation(
            Summary = "Download selected photos/videos as a zip file or single file",
            Description = "Download photos/videos from the server as a zip file or single file",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult> HandleAsync([FromBody] DownloadObjectsRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult>(
            fileResult => File(fileResult.FileBytes, fileResult.ContentType, fileResult.FileName),
            notFound => new NoContentResult()
        );
    }
}

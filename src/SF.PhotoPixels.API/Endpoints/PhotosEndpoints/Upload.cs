using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Extensions;
using SF.PhotoPixels.Application.Commands;
using SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;
using SF.PhotoPixels.Application.Commands.VideoStorage.StoreVideo;
using SF.PhotoPixels.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class Upload : EndpointBaseAsync.WithRequest<StoreMediaRequest>.WithActionResult<IMediaResponse>
{
    private readonly IMediator _mediator;

    public Upload(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object")]
    [SwaggerOperation(
            Summary = "Upload a photo/video",
            Description = "Uploads a photo/video to the server",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult<IMediaResponse>> HandleAsync([FromForm] StoreMediaRequest request, CancellationToken cancellationToken = default)
    {
        switch (request.File)
        {
            case var file when file.SupportFormats(Constants.SupportedPhotoFormats):
                return await UsePhotoHandler(request, cancellationToken);
            case var file when file.SupportFormats(Constants.SupportedVideoFormats):
                return await UseVideoHandler(request, cancellationToken);
            default:
                return new BadRequestResult();
        }
    }

    private async Task<ActionResult<IMediaResponse>> UseVideoHandler(StoreMediaRequest request, CancellationToken cancellationToken)
    {
        var storeVideoRequest = new StoreVideoRequest { File = request.File, ObjectHash = request.ObjectHash, AndroidCloudId = request.AndroidCloudId, AppleCloudId = request.AppleCloudId };
        var resultVideo = await _mediator.Send(storeVideoRequest, cancellationToken);
        return resultVideo.Match<ActionResult<IMediaResponse>>(
            response => new OkObjectResult(response),
            _ => new ConflictResult(),
            e => new BadRequestObjectResult(e)
        ) ?? new BadRequestResult();
    }

    private async Task<ActionResult<IMediaResponse>> UsePhotoHandler(StoreMediaRequest request, CancellationToken cancellationToken)
    {
        var storePhotoRequest = new StorePhotoRequest { File = request.File, ObjectHash = request.ObjectHash, AndroidCloudId = request.AndroidCloudId, AppleCloudId = request.AppleCloudId };
        var result1 = await _mediator.Send(storePhotoRequest, cancellationToken);
        return result1.Match<ActionResult<IMediaResponse>>(
            response => new OkObjectResult(response),
            _ => new ConflictResult(),
            e => new BadRequestObjectResult(e)
        ) ?? new BadRequestResult();
    }
}

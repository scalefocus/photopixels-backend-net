using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using SF.PhotoPixels.Application.Query.User.GetVideoPreviewFilesSize;
using SF.PhotoPixels.Domain.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class GetVideoConversationFilesSizeEndpoint : EndpointBaseAsync.WithoutRequest.WithoutResult
{
    private readonly IMediator _mediator;
    private readonly IFeatureManager _featureManager;

    public GetVideoConversationFilesSizeEndpoint(IMediator mediator, IFeatureManager featureManager = null)
    {
        _mediator = mediator;
        _featureManager = featureManager;
    }

    [HttpGet("/user/getvideoconversationfilessize")]
    [SwaggerOperation(
        Summary = "Get Video Conversation Files Size",
        Description = "Get Video Conversation Files Size",
        Tags = new[] { "Users" })
    ]
    public override async Task<ActionResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        if (!await _featureManager.IsEnabledAsync(ConfigurationConstants.EnableIOSVideoConverstion)) return new BadRequestObjectResult("Feature IOS Video Converstion is not enabled");

        var result = await _mediator.Send(new GetVideoPreviewFilesSizeRequest(), cancellationToken);

        return result.Match<ActionResult>(
            userVideoPreviewInfo => new OkObjectResult(userVideoPreviewInfo),
            _ => new ForbidResult()
        );
    }
}


using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.User.GetVideoPreviewFilesSize;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class GetVideoConversationFilesSizeEndpoint : EndpointBaseAsync.WithoutRequest.WithoutResult
{
    private readonly IMediator _mediator;

    public GetVideoConversationFilesSizeEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/user/getvideoconversationfilessize")]
    [SwaggerOperation(
        Summary = "Get Video Conversation Files Size",
        Description = "Get Video Conversation Files Size",
        Tags = new[] { "Users" })
    ]
    public override async Task<ActionResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetVideoPreviewFilesSizeRequest(), cancellationToken);

        return result.Match<ActionResult>(
            userVideoPreviewInfo => new OkObjectResult(userVideoPreviewInfo),
            _ => new ForbidResult()
        );
    }
}


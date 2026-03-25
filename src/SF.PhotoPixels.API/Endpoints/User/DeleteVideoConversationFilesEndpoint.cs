using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using SF.PhotoPixels.Application.Commands.User.DeleteVideoPreviewFiles;
using SF.PhotoPixels.Domain.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class DeleteVideoConversationFilesEndpoint : EndpointBaseAsync.WithoutRequest.WithoutResult
{
    private readonly IMediator _mediator;
    private readonly IFeatureManager _featureManager;

    public DeleteVideoConversationFilesEndpoint(IMediator mediator, IFeatureManager featureManager)
    {
        _mediator = mediator;
        _featureManager = featureManager;
    }

    [HttpDelete("/users/deletevideoconversationfiles")]
    [SwaggerOperation(
        Summary = "Delete Video Conversation Files",
        Tags = new[] { "Users" })
    ]
    public override async Task<ActionResult<DeleteVideoPreviewFilesResponse>> HandleAsync(CancellationToken cancellationToken = new())
    {
        if (!await _featureManager.IsEnabledAsync(ConfigurationConstants.EnableIOSVideoConverstion)) return new BadRequestObjectResult("Feature IOS Video Converstion is not enabled");

        var result = await _mediator.Send(new DeleteVideoPreviewFilesRequest(), cancellationToken);

        return result.Match<ActionResult<DeleteVideoPreviewFilesResponse>>(
            _ => Empty,
            _ => NotFound(),
            _ => Forbid()
        );
    }
}
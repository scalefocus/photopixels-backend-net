using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.User.DeleteVideoPreviewFiles;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class DeleteVideoConversationFilesEndpoint : EndpointBaseAsync.WithoutRequest.WithoutResult
{
    private readonly IMediator _mediator;

    public DeleteVideoConversationFilesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("/users/deletevideoconversationfiles")]
    [SwaggerOperation(
        Summary = "Delete Video Conversation Files",
        Tags = new[] { "Users" })
    ]
    public override async Task<ActionResult<DeleteVideoPreviewFilesResponse>> HandleAsync(CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(new DeleteVideoPreviewFilesRequest(), cancellationToken);

        return result.Match<ActionResult<DeleteVideoPreviewFilesResponse>>(
            _ => Empty,
            _ => NotFound(),
            _ => Forbid()
        );
    }
}
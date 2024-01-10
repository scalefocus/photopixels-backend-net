using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.User.DeleteUser;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class DeleteUserEndpoint : EndpointBaseAsync.WithRequest<DeleteUserRequest>.WithActionResult<DeleteUserResponse>
{
    private readonly IMediator _mediator;

    public DeleteUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("/user")]
    [SwaggerOperation(
            Summary = "Delete user",
            Tags = new[] { "Users" }),
    ]
    public override async Task<ActionResult<DeleteUserResponse>> HandleAsync([FromBody] DeleteUserRequest request, CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<DeleteUserResponse>>(
            _ => Empty,
            _ => NotFound(),
            _ => Forbid()
        );
    }
}
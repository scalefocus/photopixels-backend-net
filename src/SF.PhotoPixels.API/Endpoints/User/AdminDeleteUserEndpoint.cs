using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Commands.User.DeleteUser;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[RequireAdminRole]
public class AdminDeleteUserEndpoint : EndpointBaseAsync.WithRequest<AdminDeleteUserRequest>.WithActionResult<DeleteUserResponse>
{
    private readonly IMediator _mediator;

    public AdminDeleteUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("admin/user/{Id}")]
    [SwaggerOperation(
            Summary = "Delete user",
            Tags = new[] { "Admin" }),
    ]
    public override async Task<ActionResult<DeleteUserResponse>> HandleAsync([FromRoute] AdminDeleteUserRequest request, CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<DeleteUserResponse>>(
            _ => Empty,
            _ => NotFound(),
            _ => Forbid()
        );
    }
}
using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Commands.User.ChangeRole;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[RequireAdminRole]
public class AdminChangeRoleEndpoint : EndpointBaseAsync.WithRequest<AdminChangeRoleRequest>.WithActionResult<AdminChangeRoleResponse>
{
    private readonly IMediator _mediator;

    public AdminChangeRoleEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/admin/role")]
    [SwaggerOperation(
            Summary = "Chage user role",
            Tags = new[] { "Admin" }),
    ]
    public override async Task<ActionResult<AdminChangeRoleResponse>> HandleAsync([FromBody] AdminChangeRoleRequest request, CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult<AdminChangeRoleResponse>>(
            _ => Empty,
            _ => NotFound(),
            _ => Forbid()
        );
    }
}
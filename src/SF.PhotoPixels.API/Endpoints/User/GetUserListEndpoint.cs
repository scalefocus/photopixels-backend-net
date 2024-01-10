using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Query.User;
using SF.PhotoPixels.Application.Query.User.GetUserList;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[RequireAdminRole]
public class GetUserListEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<List<GetUserResponse>>
{
    private readonly IMediator _mediator;

    public GetUserListEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/users")]
    [SwaggerOperation(
            Summary = "Get users list",
            Description = "Get users list",
            OperationId = "Get_Users",
            Tags = new[] { "Admin" }),
    ]
    public override async Task<ActionResult<List<GetUserResponse>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUserListRequest(), cancellationToken);

        return Ok(result);
    }
}
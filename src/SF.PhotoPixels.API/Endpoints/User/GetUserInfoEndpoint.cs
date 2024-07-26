using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.User.GetUserInfo;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class GetUserInfoEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<GetUserInfoResponse>
{
    private readonly IMediator _mediator;

    public GetUserInfoEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/user/info")]
    [SwaggerOperation(
            Summary = "Get logged in user info",
            Tags = new[] { "Users" }),
    ]
    public override async Task<ActionResult<GetUserInfoResponse>> HandleAsync(CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(GetUserInfoRequest.Instance, cancellationToken);

        return result.Match<ActionResult>(
            userInfo => new OkObjectResult(userInfo),
            _ => new ForbidResult()
        );
    }
}
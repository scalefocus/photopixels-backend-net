using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.User.Login;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[AllowAnonymous]
public class LoginEndpoint : EndpointBaseAsync.WithRequest<LoginRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public LoginEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/user/login")]
    [SwaggerOperation(
            Summary = "User login",
            Tags = new[] { "Users" }),
    ]
    public override async Task<ActionResult> HandleAsync([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult>(
            _ => new EmptyResult(),
            _ => new ForbidResult());
    }
}
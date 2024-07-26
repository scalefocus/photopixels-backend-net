using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.User.RefreshToken;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[AllowAnonymous]
public class RefreshTokenEndpoint : EndpointBaseAsync.WithRequest<RefreshTokenRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public RefreshTokenEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/user/refresh")]
    [SwaggerOperation(
            Summary = "User login",
            Tags = new[] { "Users" }),
    ]
    public override async Task<ActionResult> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken = new())
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult>(
            _ => new EmptyResult(),
            _ => new ForbidResult());
    }
}
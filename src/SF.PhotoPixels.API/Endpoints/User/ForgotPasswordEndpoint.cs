using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.User.ForgotPassword;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[AllowAnonymous]
public class ForgotPasswordEndpoint : EndpointBaseAsync.WithRequest<ForgotPasswordRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public ForgotPasswordEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/user/forgotpassword")]
    [SwaggerOperation(
            Summary = "Request a reset of forgotten user password",
            Tags = new[] { "Users" }),
    ]
    public override async Task<ActionResult> HandleAsync([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult>(
            response => new OkObjectResult(response),
            _ => new NotFoundResult()
        );
    }
}
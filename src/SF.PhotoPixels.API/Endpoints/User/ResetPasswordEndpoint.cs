using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.User.ResetPassword;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[AllowAnonymous]
public class ResetPasswordEndpoint : EndpointBaseAsync.WithRequest<ResetPasswordRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public ResetPasswordEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/user/resetpassword")]
    [SwaggerOperation(
            Summary = "Reset the user password",
            Tags = new[] { "Users" }),
    ]
    public override async Task<ActionResult> HandleAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(request, cancellationToken);

        return response.Match<ActionResult>(
            response => Ok(response),
            validationError => new BadRequestObjectResult(validationError),
            BusinessLogicError => new BadRequestObjectResult(BusinessLogicError)
     );
    }
}
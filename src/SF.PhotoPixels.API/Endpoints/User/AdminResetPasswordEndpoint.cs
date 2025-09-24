using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Commands.User.ResetPassword;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[RequireAdminRole]
public class AdminResetPasswordEndpoint : EndpointBaseAsync.WithRequest<AdminResetPasswordRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public AdminResetPasswordEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/admin/resetpassword")]
    [SwaggerOperation(
            Summary = "Reset the user password",
            Tags = new[] { "Admin" }),
    ]
    public override async Task<ActionResult> HandleAsync([FromBody] AdminResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(request, cancellationToken);

        return response.Match<ActionResult>(
            response => Ok(response),
            validationError => new BadRequestObjectResult(validationError.Errors),
            BusinessLogicError => new BadRequestObjectResult(BusinessLogicError.Errors)
        );
    }
}
using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Commands.User.Register;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[RequireAdminRole]
public class AdminRegisterEndpoint : EndpointBaseAsync.WithRequest<AdminRegisterRequest>.WithResult<IResult>
{
    private readonly IMediator _mediator;

    public AdminRegisterEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/admin/register")]
    [SwaggerOperation(
            Summary = "Register User",
            Tags = new[] { "Admin" }),
    ]
    public override async Task<IResult> HandleAsync([FromBody] AdminRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<IResult>(
            _ => Results.Ok(),
            validationError => Results.ValidationProblem(validationError.Errors)
        );
    }
}
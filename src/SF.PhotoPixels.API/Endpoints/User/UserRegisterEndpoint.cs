using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.User.Register;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

[AllowAnonymous]
public class UserRegisterEndpoint : EndpointBaseAsync.WithRequest<UserRegisterRequest>.WithResult<IResult>
{
    private readonly IMediator _mediator;

    public UserRegisterEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/user/register")]
    [SwaggerOperation(
            Summary = "Register User",
            Tags = new[] { "Users" }),
    ]
    public override async Task<IResult> HandleAsync([FromBody] UserRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<IResult>(
            _ => Results.Ok(),
            validationError => Results.ValidationProblem(validationError.Errors)
        );
    }
}
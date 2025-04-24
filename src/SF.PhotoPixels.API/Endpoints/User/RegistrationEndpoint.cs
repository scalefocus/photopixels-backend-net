using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class RegistrationEndpoint : EndpointBaseAsync.WithRequest<RegistrationRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public RegistrationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RequireAdminRole]
    [HttpPost("/registration")]
    [SwaggerOperation(
            Summary = "Disable registration of new users",
            Tags = new[] { "Admin" }),
    ]
    public override async Task<ActionResult> HandleAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<ActionResult>(_ => new OkResult());
    }
}
using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Query.GetLogs;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

#if DEBUG
[AllowAnonymous]
#else
[RequireAdminRole]
#endif

public class GetLogsEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<FileStream>
{
    private readonly IMediator _mediator;

    public GetLogsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/logs")]
    [SwaggerOperation(
            Summary = "Get logs",
            Description = "Get server logs for the past day",
            Tags = new[] { "Status" }),
    ]
    public override async Task<ActionResult<FileStream>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(GetLogsRequest.Instance, cancellationToken);

        return result.Match<ActionResult<FileStream>>(
            x => new OkObjectResult(x),
            _ => new NoContentResult()
        );
    }
}
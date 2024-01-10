using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SF.PhotoPixels.Application.Query.GetStatus;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace SF.PhotoPixels.API.Endpoints.User;

[AllowAnonymous]
public class GetStatusEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<GetStatusResponse>
{
    private readonly IMediator _mediator;

    public GetStatusEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/status")]
    [SwaggerOperation(
            Summary = "Get status",
            Description = "Get server version and registration lock information",
            Tags = new[] { "Status" }),
    ]
    public override async Task<ActionResult<GetStatusResponse>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(GetStatusRequest.Instance, cancellationToken);

        return result.Match<ActionResult<GetStatusResponse>>(
            x => RemoveNullsFromResponse(x),
            _ => new NotFoundResult()
        );
    }

    private static OkObjectResult RemoveNullsFromResponse(GetStatusResponse x)
    {
        return new OkObjectResult(JsonSerializer.Serialize(x,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = true,
                    }));
    }
}
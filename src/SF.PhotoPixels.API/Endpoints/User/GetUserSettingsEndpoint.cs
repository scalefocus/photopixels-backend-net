using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.User.GetUserSettings;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class GetUserSettingsEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<GetUserSettingsResponse>
{
    private readonly IMediator _mediator;

    public GetUserSettingsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/users/me/settings")]
    [SwaggerOperation(
            Summary = "Gets the current user settings",
            Description = "Gets the current user settings",
            OperationId = "Get_Current_User_Settings",
            Tags = new[] { "Users" }),
    ]
    public override async Task<ActionResult<GetUserSettingsResponse>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUserSettingsRequest(), cancellationToken);
        return Ok(result);
    }
}

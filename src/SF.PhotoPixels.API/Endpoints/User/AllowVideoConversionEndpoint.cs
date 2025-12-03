using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.User.AllowVideoConversion;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class AllowVideoConversionEndpoint : EndpointBaseAsync.WithRequest<AllowVideoConversionRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public AllowVideoConversionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("/user/allowvideoconversion/{AllowVideoConversion}")]
    [SwaggerOperation(
        Summary = "Set the option for the user to be able to convert vidoes for preview",
        Description = "Set the user settings, which allows the user to convert ihpone viode preview",
        OperationId = "",
        Tags = new[] { "Users" })
    ]
    public override async Task<ActionResult> HandleAsync([FromRoute] AllowVideoConversionRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(request, cancellationToken);

        return response.Match<ActionResult>(
            response => Ok(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}

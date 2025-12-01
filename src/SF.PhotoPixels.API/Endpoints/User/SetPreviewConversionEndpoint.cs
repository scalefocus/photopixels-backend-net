using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.User.SetPreviewConversion;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class SetPreviewConversionEndpoint : EndpointBaseAsync.WithRequest<SetPreviewConversionRequest>.WithActionResult
{
    private readonly IMediator _mediator;

    public SetPreviewConversionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("/user/previewconversion/{PreviewConversion}")]
    [SwaggerOperation(
        Summary = "Set the option for the user to be able to convert vidoes for preview",
        Description = "Get user by id",
        OperationId = "Set_PreviewConversion",
        Tags = new[] { "Users" })
    ]
    public override async Task<ActionResult> HandleAsync([FromRoute] SetPreviewConversionRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(request, cancellationToken);

        return response.Match<ActionResult>(
            response => Ok(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}

using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using SF.PhotoPixels.Application.Commands.User.AllowVideoConversion;
using SF.PhotoPixels.Domain.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User;

public class AllowVideoConversionEndpoint : EndpointBaseAsync.WithRequest<AllowVideoConversionRequest>.WithActionResult
{
    private readonly IMediator _mediator;
    private readonly IFeatureManager _featureManager;

    public AllowVideoConversionEndpoint(IMediator mediator, IFeatureManager featureManager = null)
    {
        _mediator = mediator;
        _featureManager = featureManager;
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
        if (!await _featureManager.IsEnabledAsync(ConfigurationConstants.EnableIOSVideoConverstion)) return new BadRequestObjectResult("Feature IOS Video Converstion is not enabled");

        var response = await _mediator.Send(request, cancellationToken);

        return response.Match<ActionResult>(
            response => Ok(response),
            validationError => new BadRequestObjectResult(validationError)
        );
    }
}

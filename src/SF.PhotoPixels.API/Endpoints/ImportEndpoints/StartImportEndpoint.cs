using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.Import.StartImport;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.ImportEndpoints;

public class StartImportEndpoint : EndpointBaseAsync.WithRequest<StartImportRequest>.WithActionResult<StartImportResponse>
{
    private readonly IMediator _mediator;
    public StartImportEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/import/start")]
    [SwaggerOperation(
    Summary = "Starts directory import",
    Description = "Starts a new import of a directory's images into the users directory",
    Tags = ["Import"])]
    public override async Task<ActionResult<StartImportResponse>> HandleAsync([FromBody] StartImportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(request, cancellationToken);

        return response.Match<ActionResult<StartImportResponse>>(
            response => Ok(response),
            _ => new BadRequestResult()
            );
    }
}

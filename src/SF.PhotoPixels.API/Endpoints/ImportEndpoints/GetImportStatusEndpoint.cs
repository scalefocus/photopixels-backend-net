using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.Import;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.ImportEndpoints;

public class GetImportStatusEndpoint : EndpointBaseAsync.WithRequest<GetImportStatusRequest>.WithActionResult<GetImportStatusResponse>
{
    private readonly IMediator _mediator;

    public GetImportStatusEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/import/status")]
    [SwaggerOperation(
    Summary = "Get status",
    Description = "Get progress for an import",
    Tags = new[] { "Import" })]
    public override async Task<ActionResult<GetImportStatusResponse>> HandleAsync([FromQuery] GetImportStatusRequest request, CancellationToken cancellationToken = default)
    {

        var response = await _mediator.Send(request);

        return Ok(response);
    }
}

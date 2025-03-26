using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;

using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.TrashHardDelete;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class EmptyTrashBinEndpoint : EndpointBaseAsync
                                        .WithoutRequest
                                        .WithActionResult<EmptyTrashBinResponse>
{
    private readonly IMediator _mediator;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public EmptyTrashBinEndpoint(IMediator mediator, IExecutionContextAccessor executionContextAccessor)
    {
        _mediator = mediator;
        _executionContextAccessor = executionContextAccessor;
    }

    [HttpDelete("/emptytrash")]
    [SwaggerOperation(
            Summary = "Empty users trash bin",
            Tags = new[] { "Users" }),
    ]
    public override async Task<ActionResult<EmptyTrashBinResponse>> HandleAsync(CancellationToken cancellationToken = new())
    {
        var request = new EmptyTrashBinRequest()
        {
            UserId = _executionContextAccessor.UserId
        };

        var result = await _mediator.Send(request, cancellationToken);

        return new OkObjectResult(result);
    }
}
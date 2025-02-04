using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Query.GetUserUploads;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.TusEndpoints;

public class GetUserUploadsEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<GetUserUploadsResponse>
{
    private readonly IMediator _mediator;

    public GetUserUploadsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet("/resumable_uploads")]
    [SwaggerOperation(
        Summary = "Get User current resumable uploads",
        Tags = ["Tus"]),
]
    public override async Task<ActionResult<GetUserUploadsResponse>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetUserUploadsRequest());

        return response;
    }
}

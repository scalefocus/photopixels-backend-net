using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Commands.User.Quota;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User
{
    [RequireAdminRole]
    public class AdjustQuotaEndpoint : EndpointBaseAsync.WithRequest<AdjustQuotaRequest>.WithActionResult<AdjustQuotaResponse>
    {
        private readonly IMediator _mediator;
        public AdjustQuotaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("/admin/quota")]
        [SwaggerOperation(
                Summary = "Adjust storage quota of User",
                Tags = new[] { "Admin" }),
        ]
        public override async Task<ActionResult<AdjustQuotaResponse>> HandleAsync([FromBody] AdjustQuotaRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return result.Match<ActionResult<AdjustQuotaResponse>>(
                response => new OkObjectResult(response),
                _ => new NotFoundResult(),
                e => new BadRequestObjectResult(e)
            );
        }
    }
}

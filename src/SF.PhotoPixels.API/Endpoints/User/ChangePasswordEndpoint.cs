using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.User.ChangePassword;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User
{

    public class ChangePasswordEndpoint : EndpointBaseAsync.WithRequest<ChangePasswordRequest>.WithActionResult
    {
        private readonly IMediator _mediator;

        public ChangePasswordEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("/user/changepassword")]
        [SwaggerOperation(
                Summary = "Change the user password",
                Tags = new[] { "Users" }),
        ]
        public override async Task<ActionResult> HandleAsync([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(request, cancellationToken);

            return response.Match<ActionResult>(
                 response => Ok(response),
                 validationError => new BadRequestObjectResult(validationError),
                 BusinessLogicError => new BadRequestObjectResult(BusinessLogicError)
                 );
        }
    }
}
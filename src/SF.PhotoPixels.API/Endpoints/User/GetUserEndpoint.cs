using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application.Query.User;
using SF.PhotoPixels.Application.Query.User.GetUser;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.User
{
    [RequireAdminRole]
    public class GetUserEndpoint : EndpointBaseAsync.WithRequest<GetUserRequest>.WithActionResult<GetUserResponse>
    {
        private readonly IMediator _mediator;

        public GetUserEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("/user/{Id}")]
        [SwaggerOperation(
                Summary = "Get user",
                Description = "Get user by id",
                OperationId = "Get_User",
                Tags = new[] { "Admin" }),
        ]
        public override async Task<ActionResult<GetUserResponse>> HandleAsync([FromRoute] GetUserRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(request, cancellationToken);

            return response.Match <ActionResult<GetUserResponse>>(
                    response => Ok(response),
                    _ => new NoContentResult()
                    );
        }
    }
}
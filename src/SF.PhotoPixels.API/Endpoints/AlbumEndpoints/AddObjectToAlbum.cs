using Ardalis.ApiEndpoints;

using Mediator;

using Microsoft.AspNetCore.Mvc;

using OneOf;
using OneOf.Types;

using SF.PhotoPixels.Application;

using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints
{
    public class AddObjectToAlbum: EndpointBaseAsync.WithRequest<AddObjectToAlbumRequest>.WithActionResult<OneOf<Success, ValidationError>>
    {
        private readonly IMediator _mediator;

        public AddObjectToAlbum(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("/album/{albumId}/object/{objectId}")]
        [SwaggerOperation(
                Summary = "Add an object to an album",
                Description = "Add an object to an album",
                OperationId = "Add_Object_To_Album",
                Tags = new[] { "Album operations" }),
        ]
        public override async Task<ActionResult<OneOf<Success, ValidationError>>> HandleAsync([FromBody] AddObjectToAlbumRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return result.Match<ActionResult<OneOf<Success, ValidationError>>>(
                response => new OkObjectResult(response),
                validationError => new BadRequestObjectResult(validationError)
            );
        }
    }
}
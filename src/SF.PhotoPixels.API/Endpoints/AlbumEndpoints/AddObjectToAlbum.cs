using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Commands.AlbumObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints
{
    public class AddObjectToAlbum: EndpointBaseAsync
        .WithRequest<AddObjectToAlbumRequest>
        .WithActionResult<OneOf<Success, ValidationError>>
    {
        private readonly IMediator _mediator;

        public AddObjectToAlbum(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("/album/{albumId}/objects/")]
        [SwaggerOperation(
                Summary = "Add object(s) to an album",
                Description = "Add object(s) to an album",
                OperationId = "Add_Object_To_Album",
                Tags = new[] { "Album operations" }),
        ]
        public override async Task<ActionResult<OneOf<Success, ValidationError>>> HandleAsync(AddObjectToAlbumRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return result.Match<ActionResult<OneOf<Success, ValidationError>>>(
                response => new OkObjectResult(response),
                validationError => new BadRequestObjectResult(validationError)
            );
        }
    }
}
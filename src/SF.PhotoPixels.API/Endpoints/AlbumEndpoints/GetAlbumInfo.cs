using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Query.Album;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.AlbumEndpoints
{
    public class GetAlbumInfo: EndpointBaseAsync
        .WithRequest<GetAlbumInfoRequest>
        .WithActionResult<OneOf<GetAlbumInfoResponse, NotFound>>        
    {        
        private readonly IMediator _mediator;

        public GetAlbumInfo(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("/album/{albumId}")]
        [SwaggerOperation(
                Summary = "Gets album info",
                Description = "Gets album info",
                OperationId = "Get_Album_Info",
                Tags = new[] { "Album operations" }),
        ]
        
        public override async Task<ActionResult<OneOf<GetAlbumInfoResponse, NotFound>>> HandleAsync([FromRoute] GetAlbumInfoRequest request, CancellationToken cancellationToken = default)
        {            
            var result = await _mediator.Send(request, cancellationToken);

            return result.Match<ActionResult<OneOf<GetAlbumInfoResponse, NotFound>>>(
                response => new OkObjectResult(response),
                NotFound => new NotFoundResult()
            );
        }
    }
}
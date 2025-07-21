using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.Favorites;
using SF.PhotoPixels.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class RemoveFavoriteObjects : EndpointBaseAsync.WithRequest<FavoriteObjectsRequest>.WithoutResult
{
    private readonly IMediator _mediator;

    public RemoveFavoriteObjects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object/removeFavorites")]
    [SwaggerOperation(
            Summary = "Remove objects from favorites and vice versa",
            Description = "Remove objects from and vice versa",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult> HandleAsync([FromBody] FavoriteObjectsRequest request, CancellationToken cancellationToken = default)
    {
        request.FavoriteActionType = FavoriteActionType.RemoveFromFavorites;
        var result = await _mediator.Send(request, cancellationToken);
        return new OkObjectResult(result);
    }
}



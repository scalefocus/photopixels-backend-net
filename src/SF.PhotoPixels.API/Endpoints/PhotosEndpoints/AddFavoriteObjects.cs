using Ardalis.ApiEndpoints;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.Favorites;
using SF.PhotoPixels.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SF.PhotoPixels.API.Endpoints.PhotosEndpoints;

public class AddFavoriteObjects : EndpointBaseAsync.WithRequest<FavoriteObjectsRequest>.WithoutResult
{
    private readonly IMediator _mediator;

    public AddFavoriteObjects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/object/addFavorites")]
    [SwaggerOperation(
            Summary = "Add objects to favorites and vice versa",
            Description = "Add objects to favorites and vice versa",
            Tags = new[] { "Object operations" }),
    ]
    public override async Task<ActionResult> HandleAsync([FromBody] FavoriteObjectsRequest request, CancellationToken cancellationToken = default)
    {
        request.FavoriteActionType = FavoriteActionType.AddToFavorites;
        var result = await _mediator.Send(request, cancellationToken);
        return new OkObjectResult(result);
    }
}



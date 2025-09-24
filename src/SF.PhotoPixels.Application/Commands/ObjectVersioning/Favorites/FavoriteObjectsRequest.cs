using Mediator;
using SF.PhotoPixels.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.Favorites;

public class FavoriteObjectsRequest : IRequest<FavoriteObjectsResponse>
{
    [Required]
    public required List<string> ObjectIds { get; set; }

    public FavoriteActionType FavoriteActionType { get; set; }
}






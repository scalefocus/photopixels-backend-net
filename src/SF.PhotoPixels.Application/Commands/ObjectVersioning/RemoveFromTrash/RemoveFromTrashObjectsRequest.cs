using System.ComponentModel.DataAnnotations;
using Mediator;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;

public class RemoveFromTrashObjectsRequest : IRequest<ObjectVersioningResponse>
{
    [Required]
    public required List<string> ObjectIds { get; set; }
}

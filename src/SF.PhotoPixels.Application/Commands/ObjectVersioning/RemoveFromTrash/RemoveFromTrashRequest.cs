using System.ComponentModel.DataAnnotations;
using Mediator;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;

public class RemoveFromTrashObjectRequest : IRequest<ObjectVersioningResponse>
{
    [Required]
    public required string Id { get; set; }
}
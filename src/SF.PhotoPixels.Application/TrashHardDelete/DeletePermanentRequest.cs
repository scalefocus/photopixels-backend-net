using System.ComponentModel.DataAnnotations;
using Mediator;
using SF.PhotoPixels.Application.Commands.ObjectVersioning;

namespace SF.PhotoPixels.Application.TrashHardDelete;

public class DeletePermanentRequest : IRequest<ObjectVersioningResponse>
{
    [Required]
    public required IEnumerable<string> ObjectIds { get; set; }
}
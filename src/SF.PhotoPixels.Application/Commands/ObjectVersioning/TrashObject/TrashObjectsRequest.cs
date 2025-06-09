using Mediator;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;

public class TrashObjectsRequest : IRequest<ObjectVersioningResponse>
{
    public required List<string> ObjectIds { get; set; }
}
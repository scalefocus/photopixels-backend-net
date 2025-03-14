using Mediator;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;

public class TrashObjectRequest : IRequest<ObjectVersioningResponse>
{
    public required string Id { get; set; }
}
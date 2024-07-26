using Mediator;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;

public class DeleteObjectRequest : IRequest<ObjectVersioningResponse>
{
    public required string Id { get; set; }
}
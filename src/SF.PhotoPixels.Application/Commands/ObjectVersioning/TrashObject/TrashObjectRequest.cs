using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.TrashObject;

public class TrashObjectRequest : IRequest<ObjectVersioningResponse>
{
    [FromRoute]
    public required string Id { get; set; }
}
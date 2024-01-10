using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.UpdateObject;

public class UpdateObjectRequest : IRequest<ObjectVersioningResponse>
{
    public required string Id { get; set; }

    [FromBody]
    public required UpdateObjectRequestBody RequestBody { get; set; }

}
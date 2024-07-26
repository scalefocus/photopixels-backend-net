using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadPhoto;

public class LoadPhotoRequest : IQuery<QueryResponse<PhotoResponse>>
{
    public required string Id { get; set; }

    [FromQuery]
    public string? Format { get; set; }
}
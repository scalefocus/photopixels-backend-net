using Mediator;
using Microsoft.AspNetCore.Http;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;

public class StorePhotoRequest : IRequest<OneOf<StorePhotoResponse, Duplicate,ValidationError>>
{
    public required IFormFile File { get; set; }

    public required string ObjectHash { get; set; }

    public string? AppleCloudId { get; set; }

    public string? AndroidCloudId { get; set; }


}
using Microsoft.AspNetCore.Http;

namespace SF.PhotoPixels.Application.Commands;

public class StoreMediaRequest
{
    public required IFormFile File { get; set; }
    public required string ObjectHash { get; set; }
    public string? AppleCloudId { get; set; }
    public string? AndroidCloudId { get; set; }
}


namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectData;

public class ObjectDataResponse
{
    public required string Id { get; set; }

    public required string Thumbnail { get; set; }

    public required string ContentType { get; set; }

    public required string Hash { get; set; }

    public required string? OriginalHash { get; set; }

    public string? AppleCloudId { get; set; }

    public string? AndroidCloudId { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public DateTimeOffset DateCreated { get; set; }
}
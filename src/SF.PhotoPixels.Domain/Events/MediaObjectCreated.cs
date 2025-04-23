namespace SF.PhotoPixels.Domain.Events;

public record MediaObjectCreated
{
    public required string ObjectId { get; init; }

    public required string Name { get; init; }

    public required long Timestamp { get; init; }

    public required string Extension { get; init; }

    public required string? MimeType { get; init; }

    public required int Height { get; init; }

    public required int Width { get; init; }

    public required string Hash { get; init; }

    public string? AppleCloudId { get; set; }

    public string? AndroidCloudId { get; set; }

    public Guid UserId { get; set; }

    public long SizeInBytes { get; set; }

    public string? OriginalHash { get; set; }
}
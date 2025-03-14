namespace SF.PhotoPixels.Domain.Events;

public record MediaObjectUpdated
{
    public required string ObjectId { get; init; }

    public required string Name { get; init; }

    public required DateTimeOffset DateCreated { get; init; }

    public required string Extension { get; init; }

    public required string? MimeType { get; init; }

    public required int Height { get; init; }

    public required int Width { get; init; }

    public required string Hash { get; init; }

    public string? AppleCloudId { get; set; }

    public string? AndroidCloudId { get; set; }

    public Guid UserId { get; set; }

    public long SizeInBytes { get; set; }
}
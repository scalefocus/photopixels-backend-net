namespace SF.PhotoPixels.Domain.Events;

public record AlbumCreated
{
    public required Guid AlbumId { get; init; }
    public required string Name { get; init; }
    public required Guid UserId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public bool IsSystem { get; init; }
}
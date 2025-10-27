namespace SF.PhotoPixels.Domain.Events;

public record AlbumDeleted
{
    public required Guid AlbumId { get; init; }
    public required Guid UserId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

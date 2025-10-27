namespace SF.PhotoPixels.Domain.Events;

public record AlbumUpdated
{
    public Guid AlbumId { get; init; }
    public Guid UserId { get; init; }
    public required string Name { get; init; }
    public bool IsSystem { get; init; }
}

namespace SF.PhotoPixels.Domain.Events;

public record ObjectToAlbumDeleted
{
    public required Guid AlbumId { get; init; }
    public required string ObjectId { get; init; }
}
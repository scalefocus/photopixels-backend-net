namespace SF.PhotoPixels.Domain.Events;

public record ObjectToAlbumCreated
{
    public required Guid AlbumId { get; init; }
    public required string ObjectId { get; init; }
    public required DateTimeOffset AddedAt { get; init; }
}

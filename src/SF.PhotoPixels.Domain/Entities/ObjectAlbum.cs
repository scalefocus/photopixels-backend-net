using Marten.Metadata;
using Marten.Schema;

namespace SF.PhotoPixels.Domain.Entities;


public class ObjectAlbum
{
    public ObjectAlbum(string objectId, string albumId)
    {
        ObjectId = objectId;
        AlbumId = albumId;
        Id = $"{objectId}-{albumId}";
    }
    public string Id { get; set; } = string.Empty;
    public string ObjectId { get; set; } = string.Empty;
    public string AlbumId { get; set; } = string.Empty;

    public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
}
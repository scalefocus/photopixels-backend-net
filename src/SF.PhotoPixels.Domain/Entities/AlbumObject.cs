namespace SF.PhotoPixels.Domain.Entities;

public class AlbumObject
{
    public AlbumObject(string albumId, string objectId)
    {
        AlbumId = albumId;
        ObjectId = objectId;        
        Id = $"{albumId}-{objectId}";
    }
    public string Id { get; set; } = string.Empty;
    public string AlbumId { get; set; } = string.Empty;
    public string ObjectId { get; set; } = string.Empty;    
    public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
}
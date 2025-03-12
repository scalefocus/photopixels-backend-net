namespace SF.PhotoPixels.Domain.Entities;

public class ObjectProperties
{
    public string Id { get; set; }

    public required string Name { get; set; }

    public required DateTimeOffset DateCreated { get; set; }

    public required string Extension { get; set; }

    public required string? MimeType { get; set; }

    public required int Height { get; set; }

    public required int Width { get; set; }

    public string Hash { get; set; }

    public bool IsDeleted { get; set; }

    public Guid UserId { get; set; }

    public string? AppleCloudId { get; set; }

    public string? AndroidCloudId { get; set; }

    public long SizeInBytes { get; set; }
    
    public DateTimeOffset? TrashDate { get; set; } = null;

    // ReSharper disable once NotNullOrRequiredMemberIsNotInitialized
    internal ObjectProperties()
    {
    }

    public ObjectProperties(Guid userId, string hash) : this(new ObjectId(userId, hash))
    {
    }

    public ObjectProperties(ObjectId objectId)
    {
        Id = objectId;
        Hash = objectId.Hash;
        UserId = objectId.UserId;
    }

    public string GetThumbnailName(string extenstion = "webp")
    {
        return $"{Hash}.{extenstion}";
    }

    public string GetFileName()
    {
        return $"{Hash}.{Extension}";
    }
}
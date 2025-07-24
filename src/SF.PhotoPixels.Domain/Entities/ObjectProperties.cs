
using Marten.Metadata;
using Marten.Schema;

namespace SF.PhotoPixels.Domain.Entities;


public class ObjectProperties : ISoftDeleted
{
    public string Id { get; set; }

    public required string Name { get; set; }

    public required DateTimeOffset DateCreated { get; set; }

    public required string Extension { get; set; }

    public required string? MimeType { get; set; }

    public required int Height { get; set; }

    public required int Width { get; set; }

    // This is used in the sense of file fingerprinting and is not needed by the FEs
    public string Hash { get; set; }

    public string? OriginalHash { get; set; }

    public Guid UserId { get; set; }

    public string? AppleCloudId { get; set; }

    public string? AndroidCloudId { get; set; }

    public long SizeInBytes { get; set; }

    public bool Deleted { get; set; }

    public List<Guid> AlbumIds { get; set; } = new();

    [SoftDeletedAtMetadata]
    public DateTimeOffset? DeletedAt { get; set; }

    public bool? IsFavorite { get; set; } = false;

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
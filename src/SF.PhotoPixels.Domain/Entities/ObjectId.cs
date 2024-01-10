using System.Security.Cryptography;
using System.Text;
using SF.PhotoPixels.Domain.Utils;

namespace SF.PhotoPixels.Domain.Entities;

public record ObjectId
{
    public ObjectId(Guid userId, string hash)
    {
        Id = Base64Url.Encode(SHA1.HashData(Encoding.UTF8.GetBytes(userId + hash)));
        UserId = userId;
        Hash = hash;
    }

    public string Id { get; set; }

    internal Guid UserId { get; set; }

    internal string Hash { get; set; }

    public static implicit operator string(ObjectId objectId) => objectId.Id;
}
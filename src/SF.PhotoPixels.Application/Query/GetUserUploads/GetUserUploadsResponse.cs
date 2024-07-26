namespace SF.PhotoPixels.Application.Query.GetUserUploads;

public class UserUploadData
{
    public string FileId { get; set; }

    public long ByteOffset { get; set; }

    public long? FileSize { get; set; }

    public IReadOnlyDictionary<string, string> Metadata { get; set; }

    public DateTimeOffset? Creation { get; set; }

    public DateTimeOffset? Expiration { get; set; }
}

public class GetUserUploadsResponse
{
    public IEnumerable<UserUploadData> UserUploads { get; set; }
}



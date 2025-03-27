namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectsTrashed;

public class PropertiesTrashedResponse
{
    public required string Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }
    public DateTimeOffset? DateTrashed { get; set; }
}

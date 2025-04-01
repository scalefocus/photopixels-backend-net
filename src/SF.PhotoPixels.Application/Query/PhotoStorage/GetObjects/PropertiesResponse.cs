namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;

public class PropertiesResponse
{
    public required string Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public string? MediaType { get; set; }

}

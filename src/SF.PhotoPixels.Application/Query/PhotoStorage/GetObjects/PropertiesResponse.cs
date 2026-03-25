namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;

public class PropertiesResponse
{
    public required string Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public string? MediaType { get; set; }

    public bool? IsFavorite { get; set; }

    public DateTimeOffset? DateMediaTaken { get; set; }

    public DateTimeOffset? DateMediaCreated { get; set; }

    public DateTimeOffset? DatePhotopixelsImported { get; set; }

    public string Filename { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }

    public int Height { get; init; }

    public int Width { get; init; }

}

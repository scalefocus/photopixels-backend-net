namespace SF.PhotoPixels.Application.Query.PhotoStorage;

public class PhotoResponse
{
    public required Stream Photo { get; set; }

    public required string ContentType { get; set; }
}

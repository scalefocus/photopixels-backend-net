namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class LoadMediaResponse
{
    public required Stream Media { get; set; }

    public required string ContentType { get; set; }
}

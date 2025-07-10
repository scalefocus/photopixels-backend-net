namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class DownloadObjectsResult
{
    public required byte[] FileBytes { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
}
namespace SF.PhotoPixels.Application.Commands.VideoStorage.StoreVideo;

public class StoreVideoResponse : IMediaResponse
{
    public required string Id { get; set; }

    public required long Revision { get; set; }

    public required long Quota { get; set; }

    public required long UsedQuota { get; set; }
}
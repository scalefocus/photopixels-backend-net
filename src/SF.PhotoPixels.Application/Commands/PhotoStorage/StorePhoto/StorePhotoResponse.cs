namespace SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;

public class StorePhotoResponse
{
    public required string Id { get; set; }

    public required long Revision { get; set; }

    public required long Quota { get; set; }

    public required long UsedQuota { get; set; }
}
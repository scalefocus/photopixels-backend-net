namespace SF.PhotoPixels.Application.Query.User.GetVideoPreviewFilesSize;

public class GetVideoPreviewFilesSizeResponse
{
    //public required Guid UserId { get; set; }
    public required long Size { get; set; }

    public required long Quota { get; set; }

    public required long UsedQuota { get; set; }

    public required int NumberOfPreviewFiles { get; set; }

    public required int NumberOfFilesToConvert { get; set; }

}



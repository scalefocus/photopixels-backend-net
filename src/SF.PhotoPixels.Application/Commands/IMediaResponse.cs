namespace SF.PhotoPixels.Application.Commands;

public interface IMediaResponse
{
    public string Id { get; set; }
    public long Revision { get; set; }
    public long Quota { get; set; }
    public long UsedQuota { get; set; }
}
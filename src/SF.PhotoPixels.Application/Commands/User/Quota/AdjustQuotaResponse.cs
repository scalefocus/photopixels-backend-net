namespace SF.PhotoPixels.Application.Commands.User.Quota;

public class AdjustQuotaResponse
{
    public required long Quota { get; set; }

    public required long UsedQuota { get; set; }
}
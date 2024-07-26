namespace SF.PhotoPixels.Application.Query.User.GetUserInfo;

public class GetUserInfoResponse
{
    public required string Email { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public required long Quota { get; set; }

    public required long UsedQuota { get; set; }

    public required Dictionary<string, string> Claims { get; set; }
}
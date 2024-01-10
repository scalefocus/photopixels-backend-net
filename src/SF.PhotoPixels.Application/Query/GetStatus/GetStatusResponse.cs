namespace SF.PhotoPixels.Application.Query.GetStatus;

public class GetStatusResponse
{
    public required bool Registration { get; set; }

    public required string ServerVersion { get; set; }

    public bool? PrivacyTestMode {get; set;}
}

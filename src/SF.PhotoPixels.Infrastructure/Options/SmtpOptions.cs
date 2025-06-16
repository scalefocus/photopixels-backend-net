namespace SF.PhotoPixels.Infrastructure.Options;

public class SmtpOptions
{
    public required string Host { get; set; }

    public required string Password { get; set; }

    public required string Username { get; set; }

    public required int? Port { get; set; }

    public required bool UseSsl { get; set; } = true;

    public required bool CheckCertificateRevocation { get; set; } = true;

    public bool IsValidConfiguration()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            return false;
        }

        if (Port is null or 0)
        {
            return false;
        }

        return true;
    }
}
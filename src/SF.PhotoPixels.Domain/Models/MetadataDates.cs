namespace SF.PhotoPixels.Domain.Models;

public class MetadataDates
{
    public DateTimeOffset? dateMediaTaken { get; set; }
    public DateTimeOffset? datePhoneImported { get; set; }
    public DateTimeOffset? dateMediaCreated { get; set; }
    public DateTimeOffset? datePhotopixelsImported { get; set; }
    public DateTimeOffset? dateMediaModified { get; set; }
}

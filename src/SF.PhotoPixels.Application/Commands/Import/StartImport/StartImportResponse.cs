namespace SF.PhotoPixels.Application.Commands.Import.StartImport;

public class StartImportResponse
{
    public Guid ImportID { get; set; }

    public int QueuePosition {  get; set; }
}

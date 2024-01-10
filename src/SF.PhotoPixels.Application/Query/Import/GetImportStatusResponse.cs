namespace SF.PhotoPixels.Application.Query.Import;

public class GetImportStatusResponse
{
    public bool IsInQueue { get; set; }

    public int ImportedItems { get; set; }

    public int DuplicateItems { get; set; }

    public int TotalItems { get; set; }
}

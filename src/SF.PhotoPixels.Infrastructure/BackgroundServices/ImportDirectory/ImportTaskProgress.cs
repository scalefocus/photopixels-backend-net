namespace SF.PhotoPixels.Infrastructure.BackgroundServices.ImportDirectory;

public class ImportTaskProgress
{
    public int ProcessedFiles { get; set; }

    public int DuplicateFiles { get; set; }

    public int TotalFiles { get; set; }

    public bool IsInQueue { get; set; }
}

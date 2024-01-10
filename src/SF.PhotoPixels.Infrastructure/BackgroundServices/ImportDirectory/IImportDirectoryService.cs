namespace SF.PhotoPixels.Infrastructure.BackgroundServices.ImportDirectory;

public interface IImportDirectoryService
{
    Task<int> EnqueueImport(ImportTask task);

    Task<ImportTaskProgress?> GetProgress(Guid id);
}

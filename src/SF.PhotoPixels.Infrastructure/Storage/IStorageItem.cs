namespace SF.PhotoPixels.Infrastructure.Storage;

public interface IStorageItem
{
    Task SaveAsync(Stream stream, CancellationToken cancellationToken = default);
}
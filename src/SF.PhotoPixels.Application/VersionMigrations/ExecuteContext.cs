using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.VersionMigrations;

public class ExecuteContext : ExecuteContextBase
{
    public required string ObjectId { get; init; }

    public required string ObjectName { get; init; }

    private readonly Lazy<Task<FormattedImage>> _lazyImage;

    public required string MediaName { get; set; }

    public required User User { get; init; }

    public required Func<IStorageItem, Task> SaveImage { get; init; }

    public required Func<IStorageItem, Task> SaveThumbnail { get; init; }

    public Task<FormattedImage> FormattedImage => _lazyImage.Value;

    public ExecuteContext(Func<Task<FormattedImage>> imageLoader)
    {
        _lazyImage = new Lazy<Task<FormattedImage>>(imageLoader);
    }
}
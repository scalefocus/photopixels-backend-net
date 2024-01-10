using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Infrastructure.Services.PhotoService
{
    public interface IPhotoService
    {
        Task<long> SaveFile(RawImage rawImage, Guid userId, CancellationToken cancellationToken);

        Task<long> StoreObjectCreatedEventAsync(RawImage rawImage, long usedQuota, string filename, Guid userId, CancellationToken cancellationToken, string? AppleCloudId = null, string? AndroidCloudId = null);
    }
}
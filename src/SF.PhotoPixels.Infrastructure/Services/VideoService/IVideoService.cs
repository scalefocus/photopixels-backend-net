using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Infrastructure.Services.VideoService
{
    public interface IVideoService
    {
        Task<long> SaveFile(RawVideo rawVideo, Guid userId, CancellationToken cancellationToken, bool allowVideoConversion = false);

        Task<long> StoreObjectCreatedEventAsync(RawVideo rawVideo, long usedQuota, string filename, Guid userId, CancellationToken cancellationToken, string? AppleCloudId = null, string? AndroidCloudId = null);
    }
}